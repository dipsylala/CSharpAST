using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using CSharpAST.Core.Analysis;
using CSharpAST.Core.Processing;
using CSharpAST.Core.Output;

namespace CSharpAST.Core;

/// <summary>
/// AST Generator for analyzing C# source files and generating comprehensive AST representations.
/// This class has been refactored to use focused components for better maintainability.
/// Supports both standard and high-performance processing modes.
/// </summary>
public class ASTGenerator : IDisposable
{
    private readonly ISyntaxAnalyzer _syntaxAnalyzer;
    private readonly IFileProcessor _fileProcessor;
    private readonly IOutputManager _outputManager;
    private readonly bool _verbose;
    private bool _disposed = false;

    /// <summary>
    /// Creates a standard ASTGenerator with basic performance optimizations and unified file processing (C# + Razor + VB.NET)
    /// </summary>
    public ASTGenerator(bool verbose = false)
        : this(new CSharpSyntaxAnalyzer(), new UnifiedFileProcessor(new CSharpSyntaxAnalyzer()), new OutputManager(), verbose)
    {
    }

    /// <summary>
    /// Creates a high-performance ASTGenerator with concurrent processing and memory optimizations.
    /// Supports only C# files for maximum performance.
    /// </summary>
    public static ASTGenerator CreateOptimized(bool verbose = false, int? maxConcurrency = null)
    {
        return new ASTGenerator(
            new CSharpSyntaxAnalyzer(), 
            new ConcurrentFileProcessor(new CSharpSyntaxAnalyzer(), maxConcurrency), 
            new OutputManager(), 
            verbose);
    }

    /// <summary>
    /// Creates a high-performance ASTGenerator with concurrent processing for multi-language projects.
    /// Supports C#, VB.NET, and Razor files with concurrent processing.
    /// </summary>
    public static ASTGenerator CreateConcurrentUnified(bool verbose = false, int? maxConcurrency = null)
    {
        var analyzers = new ISyntaxAnalyzer[]
        {
            new CSharpSyntaxAnalyzer(),
            new VBSyntaxAnalyzer(),
            new RazorSyntaxAnalyzer()
        };
        
        return new ASTGenerator(
            new CSharpSyntaxAnalyzer(), 
            new ConcurrentFileProcessor(analyzers, maxConcurrency), 
            new OutputManager(), 
            verbose);
    }

    /// <summary>
    /// Creates a unified ASTGenerator that supports C#, VB.NET, and Razor files with optimized processing
    /// </summary>
    public static ASTGenerator CreateUnified(bool verbose = false, int? maxConcurrency = null)
    {
        var analyzers = new ISyntaxAnalyzer[]
        {
            new CSharpSyntaxAnalyzer(),
            new VBSyntaxAnalyzer(),
            new RazorSyntaxAnalyzer()
        };
        
        return new ASTGenerator(
            new CSharpSyntaxAnalyzer(), 
            new UnifiedFileProcessor(analyzers), 
            new OutputManager(), 
            verbose);
    }

    public ASTGenerator(ISyntaxAnalyzer syntaxAnalyzer, IFileProcessor fileProcessor, IOutputManager outputManager, bool verbose = false)
    {
        _syntaxAnalyzer = syntaxAnalyzer;
        _fileProcessor = fileProcessor;
        _outputManager = outputManager;
        _verbose = verbose;
    }

    public async Task GenerateASTAsync(string inputPath, string outputPath, string format)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            if (_verbose)
                Console.WriteLine($"Starting AST generation for: {inputPath}");

            // Ensure output directory exists
            Directory.CreateDirectory(outputPath);

            ASTAnalysis? analysis = null;

            if (File.Exists(inputPath))
            {
                var extension = Path.GetExtension(inputPath).ToLowerInvariant();
                
                if (extension == ".sln")
                {
                    // Solution file
                    analysis = await _fileProcessor.ProcessSolutionAsync(inputPath);
                }
                else if (_fileProcessor.IsProjectSupported(inputPath))
                {
                    // Project file (.csproj, .vbproj, etc.)
                    analysis = await _fileProcessor.ProcessProjectAsync(inputPath);
                }
                else if (_fileProcessor.IsFileSupported(inputPath))
                {
                    // Source file (.cs, .vb, .cshtml, etc.)
                    analysis = await _fileProcessor.ProcessFileAsync(inputPath);
                }
                else
                {
                    throw new ArgumentException($"Unsupported file type: {Path.GetExtension(inputPath)}");
                }
            }
            else if (Directory.Exists(inputPath))
            {
                analysis = await ProcessDirectoryAsync(inputPath);
            }
            else
            {
                throw new ArgumentException($"Input path does not exist: {inputPath}");
            }

            if (analysis != null)
            {
                // Construct proper output file path based on input
                string outputFilePath;
                if (File.Exists(inputPath))
                {
                    // For single file input, create output filename based on input filename
                    var inputFileName = Path.GetFileNameWithoutExtension(inputPath);
                    outputFilePath = Path.Combine(outputPath, inputFileName);
                }
                else
                {
                    // For directory input, use directory name
                    var directoryName = Path.GetFileName(inputPath.TrimEnd(Path.DirectorySeparatorChar));
                    outputFilePath = Path.Combine(outputPath, directoryName);
                }
                
                await _outputManager.WriteAsync(analysis, outputFilePath, format);
                
                stopwatch.Stop();
                if (_verbose)
                    Console.WriteLine($"AST generation completed in {stopwatch.ElapsedMilliseconds}ms. Output saved to: {outputPath}");
                else
                    Console.WriteLine($"AST generation completed. Output saved to: {outputPath}");
            }
            else
            {
                Console.WriteLine("No analysis generated.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (_verbose)
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async Task<ASTAnalysis?> ProcessDirectoryAsync(string inputDir)
    {
        // First check for project or solution files
        var solutionFiles = Directory.GetFiles(inputDir, "*.sln");
        
        // Look for project files using analyzer-driven detection
        var allProjectFiles = Directory.GetFiles(inputDir, "*.*proj"); // Get all potential project files (.csproj, .vbproj, etc.)
        var supportedProjectFiles = allProjectFiles.Where(file => _fileProcessor.IsProjectSupported(file)).ToArray();

        if (solutionFiles.Length > 0)
        {
            if (_verbose)
                Console.WriteLine($"Found solution file: {solutionFiles[0]}");
            return await _fileProcessor.ProcessSolutionAsync(solutionFiles[0]);
        }
        else if (supportedProjectFiles.Length > 0)
        {
            if (_verbose)
                Console.WriteLine($"Found project file: {supportedProjectFiles[0]}");
            return await _fileProcessor.ProcessProjectAsync(supportedProjectFiles[0]);
        }
        else
        {
            // Fallback to processing all supported files as a combined analysis
            var allFiles = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories)
                .Where(file => _fileProcessor.IsFileSupported(file))
                .ToArray();
            
            // Group files by type for better reporting
            var filesByType = allFiles.GroupBy(f => Path.GetExtension(f).ToLowerInvariant()).ToList();
            
            if (_verbose)
            {
                foreach (var group in filesByType)
                {
                    var fileType = group.Key switch
                    {
                        ".cs" => "C#",
                        ".vb" => "VB.NET",
                        ".cshtml" => "Razor",
                        _ => group.Key
                    };
                    Console.WriteLine($"Found {group.Count()} {fileType} files to process");
                }
            }

            if (allFiles.Length == 0)
                return null;

            var combinedAnalysis = new ASTAnalysis
            {
                SourceFile = inputDir,
                GeneratedAt = DateTime.UtcNow,
                RootNode = new ASTNode
                {
                    Type = "DirectoryRoot",
                    Kind = "Directory",
                    Text = $"Directory: {Path.GetFileName(inputDir)}",
                    Properties = new Dictionary<string, object>
                    {
                        ["DirectoryPath"] = inputDir,
                        ["TotalFileCount"] = allFiles.Length,
                        ["FileTypes"] = string.Join(", ", filesByType.Select(g => $"{g.Key}({g.Count()})"))
                    },
                    Children = new List<ASTNode>()
                }
            };

            foreach (var file in allFiles)
            {
                var fileAnalysis = await _fileProcessor.ProcessFileAsync(file);
                if (fileAnalysis?.RootNode != null)
                {
                    combinedAnalysis.RootNode.Children.Add(fileAnalysis.RootNode);
                }
            }

            return combinedAnalysis;
        }
    }

    // Legacy methods for backward compatibility with existing tests and CLI
    public async Task<ASTAnalysis?> ProcessFileAsync(string filePath)
    {
        return await _fileProcessor.ProcessFileAsync(filePath);
    }

    public async Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        return await _fileProcessor.ProcessProjectAsync(projectPath, cancellationToken);
    }

    public async Task<ASTAnalysis?> ProcessSolutionAsync(string solutionPath, CancellationToken cancellationToken = default)
    {
        return await _fileProcessor.ProcessSolutionAsync(solutionPath, cancellationToken);
    }

    public ASTNode AnalyzeNode(SyntaxNode node)
    {
        return _syntaxAnalyzer.AnalyzeNode(node);
    }

    public async Task WriteOutputAsync(ASTAnalysis analysis, string outputPath)
    {
        await _outputManager.WriteOutputAsync(analysis, outputPath);
    }

    // Additional legacy methods used by integration tests
    public async Task<ASTAnalysis?> GenerateFromFileAsync(string filePath)
    {
        return await _fileProcessor.ProcessFileAsync(filePath);
    }

    public async Task<ProjectAnalysis?> GenerateFromProjectAsync(string projectPath)
    {
        ASTAnalysis? analysis;

        // Check if it's a directory or a specific project file
        if (Directory.Exists(projectPath))
        {
            analysis = await ProcessDirectoryAsync(projectPath);
        }
        else if (File.Exists(projectPath) && _fileProcessor.IsProjectSupported(projectPath))
        {
            analysis = await _fileProcessor.ProcessProjectAsync(projectPath);
        }
        else
        {
            return null;
        }

        if (analysis == null) return null;

        // Convert ASTAnalysis to ProjectAnalysis for backward compatibility
        var projectAnalysis = new ProjectAnalysis
        {
            ProjectPath = projectPath,
            ProjectName = Path.GetFileNameWithoutExtension(projectPath),
            GeneratedAt = analysis.GeneratedAt,
            Files = new List<ASTAnalysis>(),
            Dependencies = new List<string>(),
            TestClasses = new List<ClassInfo>(),
            AsyncPatterns = new List<AsyncPatternInfo>()
        };

        // Extract individual file analyses from the root node children
        foreach (var child in analysis.RootNode.Children)
        {
            var fileAnalysis = new ASTAnalysis
            {
                SourceFile = child.Properties.ContainsKey("FilePath") ? child.Properties["FilePath"].ToString() ?? "" : "",
                GeneratedAt = analysis.GeneratedAt,
                RootNode = child
            };
            projectAnalysis.Files.Add(fileAnalysis);
        }

        return projectAnalysis;
    }

    public ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath)
    {
        return _syntaxAnalyzer.AnalyzeSyntaxTree(root, filePath);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_fileProcessor is IDisposable disposableProcessor)
                {
                    disposableProcessor.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
