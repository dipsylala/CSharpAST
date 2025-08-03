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
    /// Creates a standard ASTGenerator with basic performance optimizations and unified file processing (C# + Razor)
    /// </summary>
    public ASTGenerator(bool verbose = false) 
        : this(new SyntaxAnalyzer(), new UnifiedFileProcessor(new SyntaxAnalyzer()), new OutputManager(), verbose)
    {
    }

    /// <summary>
    /// Creates a high-performance ASTGenerator with concurrent processing and memory optimizations
    /// </summary>
    public static ASTGenerator CreateOptimized(bool verbose = false, int? maxConcurrency = null)
    {
        return new ASTGenerator(
            new OptimizedSyntaxAnalyzer(), 
            new ConcurrentFileProcessor(new OptimizedSyntaxAnalyzer(), maxConcurrency), 
            new OutputManager(), 
            verbose);
    }

    /// <summary>
    /// Creates a unified ASTGenerator that supports both C# and Razor/CSHTML files with optimized processing
    /// </summary>
    public static ASTGenerator CreateUnified(bool verbose = false, int? maxConcurrency = null)
    {
        return new ASTGenerator(
            new OptimizedSyntaxAnalyzer(), 
            new UnifiedFileProcessor(new OptimizedSyntaxAnalyzer()), 
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
                if (!_fileProcessor.IsFileSupported(inputPath))
                {
                    throw new ArgumentException($"Unsupported file type: {Path.GetExtension(inputPath)}");
                }

                analysis = await _fileProcessor.ProcessFileAsync(inputPath);
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
        var projectFiles = Directory.GetFiles(inputDir, "*.csproj");

        if (solutionFiles.Length > 0)
        {
            if (_verbose)
                Console.WriteLine($"Found solution file: {solutionFiles[0]}");
            return await _fileProcessor.ProcessSolutionAsync(solutionFiles[0]);
        }
        else if (projectFiles.Length > 0)
        {
            if (_verbose)
                Console.WriteLine($"Found project file: {projectFiles[0]}");
            return await _fileProcessor.ProcessProjectAsync(projectFiles[0]);
        }
        else
        {
            // Fallback to processing all .cs files as a combined analysis
            var csFiles = Directory.GetFiles(inputDir, "*.cs", SearchOption.AllDirectories);
            
            if (_verbose)
                Console.WriteLine($"Found {csFiles.Length} C# files to process");

            if (csFiles.Length == 0)
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
                        ["FileCount"] = csFiles.Length
                    },
                    Children = new List<ASTNode>()
                }
            };

            foreach (var file in csFiles)
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
        else if (File.Exists(projectPath) && projectPath.EndsWith(".csproj"))
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
