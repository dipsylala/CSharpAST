using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using CSharpAST.Core.Analysis;

namespace CSharpAST.Core.Processing;

/// <summary>
/// High-performance concurrent file processor for AST generation.
/// Uses parallel processing, connection pooling, and optimized memory management.
/// Supports multiple analyzers for multi-language projects.
/// </summary>
public class ConcurrentFileProcessor : IFileProcessor
{
    private readonly List<ISyntaxAnalyzer> _analyzers;
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly int _maxConcurrency;

    public ConcurrentFileProcessor(ISyntaxAnalyzer syntaxAnalyzer, int? maxConcurrency = null)
        : this(new[] { syntaxAnalyzer }, maxConcurrency)
    {
    }

    public ConcurrentFileProcessor(IEnumerable<ISyntaxAnalyzer> analyzers, int? maxConcurrency = null)
    {
        _analyzers = analyzers?.ToList() ?? throw new ArgumentNullException(nameof(analyzers));
        if (_analyzers.Count == 0)
            throw new ArgumentException("At least one analyzer must be provided", nameof(analyzers));
            
        _maxConcurrency = maxConcurrency ?? Math.Min(Environment.ProcessorCount * 2, 16);
        _concurrencyLimiter = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);
    }

    public async Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Legacy method for backward compatibility - delegates to ProcessFileAsync
        return await ProcessFileAsync(filePath);
    }

    public async Task<ASTAnalysis?> ProcessFileAsync(string filePath)
    {
        await _concurrencyLimiter.WaitAsync();
        try
        {
            if (!File.Exists(filePath))
                return null;

            // Find the appropriate analyzer for this file type using capabilities
            var analyzer = _analyzers.FirstOrDefault(a => a.Capabilities.SupportsFile(filePath));
            if (analyzer == null)
                return null;

            // Use ConfigureAwait(false) for better thread pool utilization
            var sourceText = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            
            return await Task.Run(() => analyzer.AnalyzeFile(filePath, sourceText));
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    public async Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(projectPath))
            return null;

        var projectDir = Path.GetDirectoryName(projectPath);
        if (string.IsNullOrEmpty(projectDir))
            return null;

        // Extract source files that are actually included in the project
        var includedFiles = ProjectFileParser.GetIncludedSourceFiles(projectPath, _analyzers);

        // Debug: Log the number of files found
        Console.WriteLine($"Debug: Found {includedFiles.Count} files in project {Path.GetFileName(projectPath)}");
        foreach (var file in includedFiles.Take(5)) // Show first 5 files
        {
            Console.WriteLine($"Debug: File found: {Path.GetFileName(file)}");
        }
        if (includedFiles.Count > 5)
        {
            Console.WriteLine($"Debug: ... and {includedFiles.Count - 5} more files");
        }

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
                    ["FileCount"] = includedFiles.Count,
                    ["ProjectType"] = Path.GetExtension(projectPath),
                    ["ParsedFromProjectFile"] = true,
                    ["SupportedExtensions"] = string.Join(", ", _analyzers.SelectMany(a => GetSupportedExtensions(a)).Distinct())
                },
                Children = new List<ASTNode>()
            }
        };

        // Use concurrent processing with proper error handling
        var fileResults = await ProcessFilesConcurrentlyAsync(includedFiles, cancellationToken);
        
        // Debug: Log processing results
        Console.WriteLine($"Debug: Processed {fileResults.Count} files, successful: {fileResults.Count(r => r.Analysis != null)}, errors: {fileResults.Count(r => r.Error != null)}");
        
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

        // Parse solution file to extract project files
        var solutionInfo = SolutionFileParser.GetSolutionInfo(solutionPath);
        var projectFiles = solutionInfo.ProjectFiles;
        
        var analysis = new ASTAnalysis
        {
            SourceFile = solutionPath,
            GeneratedAt = DateTime.UtcNow,
            RootNode = new ASTNode
            {
                Type = "SolutionRoot",
                Kind = "Solution",
                Text = $"Solution: {solutionInfo.Name}",
                Properties = new Dictionary<string, object>
                {
                    ["SolutionPath"] = solutionPath,
                    ["ProjectCount"] = projectFiles.Count,
                    ["FormatVersion"] = solutionInfo.FormatVersion ?? "Unknown",
                    ["VisualStudioVersion"] = solutionInfo.VisualStudioVersion ?? "Unknown"
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
                    Text = $"Failed to process project {Path.GetFileName(result.ProjectFile)}: {result.Error.Message}",
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
        var tasks = filePaths.Select(async filePath =>
        {
            try
            {
                var analysis = await ProcessFileAsync(filePath).ConfigureAwait(false);
                return (filePath, analysis, (Exception?)null);
            }
            catch (Exception ex)
            {
                return (filePath, (ASTAnalysis?)null, ex);
            }
        });

        var results = await Task.WhenAll(tasks);
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
                    var analysis = await ProcessFileAsync(filePath);
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
               _analyzers.Any(analyzer => analyzer.Capabilities.SupportsFile(filePath));
    }

    public bool IsProjectSupported(string projectPath)
    {
        return !string.IsNullOrEmpty(projectPath) && 
               File.Exists(projectPath) && 
               _analyzers.Any(analyzer => analyzer.Capabilities.SupportsProject(projectPath));
    }

    private List<string> GetProjectFiles(string directoryPath)
    {
        // Find all potential project files and filter by analyzer support using capabilities
        var allFiles = Directory.GetFiles(directoryPath, "*.*proj", SearchOption.AllDirectories);
        return allFiles.Where(file => _analyzers.Any(analyzer => analyzer.Capabilities.SupportsProject(file))).ToList();
    }

    private static IEnumerable<string> GetSupportedExtensions(ISyntaxAnalyzer analyzer)
    {
        // Return the supported extensions directly from capabilities
        return analyzer.Capabilities.SupportedFileExtensions;
    }

    public void Dispose()
    {
        _concurrencyLimiter?.Dispose();
    }
}
