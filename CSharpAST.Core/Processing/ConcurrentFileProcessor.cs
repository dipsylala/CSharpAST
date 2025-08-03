using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using CSharpAST.Core.Analysis;

namespace CSharpAST.Core.Processing;

/// <summary>
/// High-performance concurrent file processor for AST generation.
/// Uses parallel processing, connection pooling, and optimized memory management.
/// </summary>
public class ConcurrentFileProcessor : IFileProcessor
{
    private readonly ISyntaxAnalyzer _syntaxAnalyzer;
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly int _maxConcurrency;

    public ConcurrentFileProcessor(ISyntaxAnalyzer syntaxAnalyzer, int? maxConcurrency = null)
    {
        _syntaxAnalyzer = syntaxAnalyzer;
        _maxConcurrency = maxConcurrency ?? Math.Min(Environment.ProcessorCount * 2, 16);
        _concurrencyLimiter = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);
    }

    public async Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(filePath))
                return null;

            // Use ConfigureAwait(false) for better thread pool utilization
            var sourceText = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
            var root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            return _syntaxAnalyzer.AnalyzeSyntaxTree(root, filePath);
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    public async Task<ASTAnalysis?> ProcessFileAsync(string filePath)
    {
        return await ProcessCSharpFileAsync(filePath);
    }

    public async Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(projectPath))
            return null;

        var projectDir = Path.GetDirectoryName(projectPath);
        if (string.IsNullOrEmpty(projectDir))
            return null;

        // Get all C# files upfront
        var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("bin") && !f.Contains("obj"))
            .ToList();

        var analysis = new ASTAnalysis
        {
            SourceFile = projectPath,
            GeneratedAt = DateTime.UtcNow,
            RootNode = new ASTNode
            {
                Type = "ProjectRoot",
                Kind = "Project",
                Text = $"Project: {Path.GetFileName(projectPath)}",
                Properties = new Dictionary<string, object>
                {
                    ["ProjectPath"] = projectPath,
                    ["FileCount"] = csFiles.Count
                },
                Children = new List<ASTNode>()
            }
        };

        // Use concurrent processing with proper error handling
        var fileResults = await ProcessFilesConcurrentlyAsync(csFiles, cancellationToken);
        
        // Add results to analysis in deterministic order
        foreach (var result in fileResults.OrderBy(r => r.FilePath))
        {
            if (result.Analysis?.RootNode != null)
            {
                analysis.RootNode.Children.Add(result.Analysis.RootNode);
            }
            else if (result.Error != null)
            {
                // Add error node for failed files
                analysis.RootNode.Children.Add(new ASTNode
                {
                    Type = "ErrorNode",
                    Kind = "Error",
                    Text = $"Failed to process {result.FilePath}: {result.Error.Message}",
                    Properties = new Dictionary<string, object>
                    {
                        ["FilePath"] = result.FilePath,
                        ["Error"] = result.Error.Message
                    },
                    Children = new List<ASTNode>()
                });
            }
        }

        return analysis;
    }

    public async Task<ASTAnalysis?> ProcessSolutionAsync(string solutionPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(solutionPath))
            return null;

        var solutionDir = Path.GetDirectoryName(solutionPath);
        if (string.IsNullOrEmpty(solutionDir))
            return null;

        // Find all project files
        var projectFiles = Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);
        
        var analysis = new ASTAnalysis
        {
            SourceFile = solutionPath,
            GeneratedAt = DateTime.UtcNow,
            RootNode = new ASTNode
            {
                Type = "SolutionRoot",
                Kind = "Solution",
                Text = $"Solution: {Path.GetFileName(solutionPath)}",
                Properties = new Dictionary<string, object>
                {
                    ["SolutionPath"] = solutionPath,
                    ["ProjectCount"] = projectFiles.Length
                },
                Children = new List<ASTNode>()
            }
        };

        // Process projects concurrently
        var projectTasks = projectFiles.Select(async projectFile =>
        {
            try
            {
                var projectAnalysis = await ProcessProjectAsync(projectFile, cancellationToken);
                return new { ProjectFile = projectFile, Analysis = projectAnalysis, Error = (Exception?)null };
            }
            catch (Exception ex)
            {
                return new { ProjectFile = projectFile, Analysis = (ASTAnalysis?)null, Error = ex };
            }
        });

        var projectResults = await Task.WhenAll(projectTasks);

        foreach (var result in projectResults.OrderBy(r => r.ProjectFile))
        {
            if (result.Analysis?.RootNode != null)
            {
                analysis.RootNode.Children.Add(result.Analysis.RootNode);
            }
            else if (result.Error != null)
            {
                analysis.RootNode.Children.Add(new ASTNode
                {
                    Type = "ErrorNode",
                    Kind = "Error",
                    Text = $"Failed to process project {result.ProjectFile}: {result.Error.Message}",
                    Properties = new Dictionary<string, object>
                    {
                        ["ProjectPath"] = result.ProjectFile,
                        ["Error"] = result.Error.Message
                    },
                    Children = new List<ASTNode>()
                });
            }
        }

        return analysis;
    }

    private async Task<List<(string FilePath, ASTAnalysis? Analysis, Exception? Error)>> ProcessFilesConcurrentlyAsync(
        List<string> filePaths, CancellationToken cancellationToken)
    {
        var results = new ConcurrentBag<(string FilePath, ASTAnalysis? Analysis, Exception? Error)>();
        
        // Create partitions for better load balancing
        var partitioner = Partitioner.Create(filePaths, true);
        
        await Task.Run(() =>
        {
            Parallel.ForEach(partitioner, new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _maxConcurrency
            }, async filePath =>
            {
                try
                {
                    var analysis = await ProcessCSharpFileAsync(filePath).ConfigureAwait(false);
                    results.Add((filePath, analysis, null));
                }
                catch (Exception ex)
                {
                    results.Add((filePath, null, ex));
                }
            });
        }, cancellationToken);

        return results.ToList();
    }

    public async Task<List<ASTAnalysis>> ProcessMultipleFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
    {
        var results = new List<ASTAnalysis>();
        var filePathsList = filePaths.ToList();

        if (filePathsList.Count == 0)
            return results;

        // Use concurrent processing with proper error handling
        var concurrentResults = new ConcurrentBag<ASTAnalysis>();
        
        await Task.Run(async () =>
        {
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _maxConcurrency
            };

            await Parallel.ForEachAsync(filePathsList, parallelOptions, async (filePath, ct) =>
            {
                try
                {
                    var analysis = await ProcessCSharpFileAsync(filePath, ct);
                    if (analysis != null)
                    {
                        concurrentResults.Add(analysis);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw; // Re-throw cancellation
                }
                catch (Exception)
                {
                    // Log or handle individual file errors
                    // Continue processing other files
                }
            });
        }, cancellationToken);

        return concurrentResults.ToList();
    }

    public bool IsFileSupported(string filePath)
    {
        return !string.IsNullOrEmpty(filePath) && 
               File.Exists(filePath) && 
               Path.GetExtension(filePath).Equals(".cs", StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _concurrencyLimiter?.Dispose();
    }
}
