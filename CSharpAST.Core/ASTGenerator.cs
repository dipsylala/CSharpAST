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
/// AST Generator responsible for orchestrating the analysis of different input types.
/// This class determines the appropriate analyzers and processors based on the input 
/// (solution, project, or individual files) and wires them together for optimal processing.
/// </summary>
public class ASTGenerator : IDisposable
{
    private readonly IOutputManager _outputManager;
    private readonly bool _verbose;
    private readonly int? _maxConcurrency;
    private readonly ProcessingMode _processingMode;
    private bool _disposed = false;

    public enum ProcessingMode
    {
        /// <summary>Sequential processing, supports all file types</summary>
        Unified,
        /// <summary>Concurrent processing, supports all file types</summary>
        Concurrent
    }

    /// <summary>
    /// Creates an ASTGenerator with the specified processing mode and output manager
    /// </summary>
    public ASTGenerator(IOutputManager outputManager, ProcessingMode mode = ProcessingMode.Concurrent, bool verbose = false, int? maxConcurrency = null)
    {
        _outputManager = outputManager ?? throw new ArgumentNullException(nameof(outputManager));
        _verbose = verbose;
        _maxConcurrency = maxConcurrency;
        _processingMode = mode;
    }

    /// <summary>
    /// Factory method to create a unified ASTGenerator for sequential processing
    /// </summary>
    public static ASTGenerator CreateUnified(IOutputManager outputManager, bool verbose = false, int? maxConcurrency = null)
    {
        return new ASTGenerator(outputManager, ProcessingMode.Unified, verbose, maxConcurrency);
    }

    public async Task GenerateASTAsync(string inputPath, string outputPath)
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
                    // Solution file - wire up appropriate processor
                    analysis = await ProcessSolutionAsync(inputPath);
                }
                else if (IsProjectFile(inputPath))
                {
                    // Project file (.csproj, .vbproj, etc.) - wire up appropriate processor
                    analysis = await ProcessProjectAsync(inputPath);
                }
                else if (IsSourceFile(inputPath))
                {
                    // Source file (.cs, .vb, .cshtml, etc.) - wire up appropriate analyzer
                    analysis = await ProcessFileAsync(inputPath);
                }
                else
                {
                    throw new ArgumentException($"Unsupported file type: {Path.GetExtension(inputPath)}");
                }
            }
            else
            {
                throw new ArgumentException($"Input path does not exist: {inputPath}");
            }

            if (analysis != null)
            {
                var extension = Path.GetExtension(inputPath).ToLowerInvariant();
                
                if (extension == ".sln")
                {
                    // For solution files, use structured output to preserve directory structure
                    await _outputManager.WriteStructuredOutputAsync(analysis, outputPath, Path.GetDirectoryName(inputPath));
                }
                else if (IsProjectFile(inputPath))
                {
                    // For project files, use structured output to preserve directory structure
                    await _outputManager.WriteStructuredOutputAsync(analysis, outputPath, Path.GetDirectoryName(inputPath));
                }
                else
                {
                    // For individual source files, create output filename based on input filename
                    var inputFileName = Path.GetFileNameWithoutExtension(inputPath);
                    var outputFilePath = Path.Combine(outputPath, inputFileName);
                    
                    // Use regular output for single files
                    await _outputManager.WriteAsync(analysis, outputFilePath);
                }
                
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

    /// <summary>
    /// Determines if a file is a supported project file type by checking analyzer capabilities
    /// </summary>
    private bool IsProjectFile(string filePath)
    {
        return AnalyzerRegistry.IsProjectSupported(filePath);
    }

    /// <summary>
    /// Determines if a file is a supported source file type by checking analyzer capabilities
    /// </summary>
    private bool IsSourceFile(string filePath)
    {
        return AnalyzerRegistry.IsFileSupported(filePath);
    }

    /// <summary>
    /// Creates the appropriate file processor based on processing mode and input requirements
    /// </summary>
    private IFileProcessor CreateFileProcessor(string inputPath)
    {
        // Determine what analyzers are needed based on the input
        var requiredAnalyzers = DetermineRequiredAnalyzers(inputPath);
        
        return _processingMode switch
        {
            ProcessingMode.Concurrent => new ConcurrentFileProcessor(requiredAnalyzers, _maxConcurrency),
            ProcessingMode.Unified => new UnifiedFileProcessor(requiredAnalyzers),
            _ => throw new ArgumentException($"Unsupported processing mode: {_processingMode}")
        };
    }

    /// <summary>
    /// Determines which syntax analyzers are needed based on the input path
    /// </summary>
    private ISyntaxAnalyzer[] DetermineRequiredAnalyzers(string inputPath)
    {
        var analyzers = new List<ISyntaxAnalyzer>();

        // For solutions and projects, scan for file types
        var extension = Path.GetExtension(inputPath).ToLowerInvariant();
        
        if (extension == ".sln")
        {
            // For solution files, include all analyzers as we don't know what's inside
            analyzers.AddRange(AnalyzerRegistry.GetAllAnalyzers());
        }
        else if (IsProjectFile(inputPath))
        {
            // For project files, find analyzers that support this project type or can process files within it
            analyzers.AddRange(AnalyzerRegistry.GetAnalyzersForProject(inputPath));
            
            // For C# projects, also include analyzers that might process files within the project
            if (extension == ".csproj")
            {
                var razorAnalyzer = AnalyzerRegistry.GetAllAnalyzers()
                    .FirstOrDefault(a => a.GetType().Name == "RazorSyntaxAnalyzer");
                if (razorAnalyzer != null && !analyzers.Contains(razorAnalyzer))
                {
                    analyzers.Add(razorAnalyzer);
                }
            }
            
            // Ensure we have at least one analyzer for any project file
            if (!analyzers.Any())
            {
                var fallbackAnalyzer = AnalyzerRegistry.GetAllAnalyzers().FirstOrDefault();
                if (fallbackAnalyzer != null)
                {
                    analyzers.Add(fallbackAnalyzer);
                }
            }
        }
        else
        {
            // For individual source files, add the specific analyzer
            var fileAnalyzer = AnalyzerRegistry.GetAnalyzerForFile(inputPath);
            analyzers.Add(fileAnalyzer);
        }

        return analyzers.ToArray();
    }

    /// <summary>
    /// Gets all available syntax analyzers from the registry
    /// </summary>
    private ISyntaxAnalyzer[] GetAllAnalyzers()
    {
        return AnalyzerRegistry.GetAllAnalyzers();
    }

    /// <summary>
    /// Gets the appropriate syntax analyzer for a specific file from the registry
    /// </summary>
    private ISyntaxAnalyzer GetAnalyzerForFile(string filePath)
    {
        return AnalyzerRegistry.GetAnalyzerForFile(filePath);
    }

    // Public methods that orchestrate the appropriate processor creation
    public async Task<ASTAnalysis?> ProcessFileAsync(string filePath)
    {
        using var fileProcessor = CreateFileProcessor(filePath);
        return await fileProcessor.ProcessFileAsync(filePath);
    }

    public async Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        using var fileProcessor = CreateFileProcessor(projectPath);
        
        // Provide feedback about project processing
        if (File.Exists(projectPath))
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            Console.WriteLine($"Processing project: {projectName}");
            
            try
            {
                // Get project source files to show what will be processed
                var analyzers = DetermineRequiredAnalyzers(projectPath);
                var sourceFiles = ProjectFileParser.GetIncludedSourceFiles(projectPath, analyzers);
                
                if (sourceFiles.Any())
                {
                    Console.WriteLine($"Found {sourceFiles.Count} source file(s) in project:");
                    foreach (var sourceFile in sourceFiles.Take(10)) // Limit to first 10 to avoid spam
                    {
                        var fileName = Path.GetFileName(sourceFile);
                        Console.WriteLine($"  • {fileName}");
                    }
                    
                    if (sourceFiles.Count > 10)
                    {
                        Console.WriteLine($"  ... and {sourceFiles.Count - 10} more file(s)");
                    }
                }
                else
                {
                    Console.WriteLine("Warning: No source files found in project.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not analyze project structure: {ex.Message}");
            }
        }
        
        return await fileProcessor.ProcessProjectAsync(projectPath, cancellationToken);
    }

    public async Task<ASTAnalysis?> ProcessSolutionAsync(string solutionPath, CancellationToken cancellationToken = default)
    {
        using var fileProcessor = CreateFileProcessor(solutionPath);
        
        // First, get the solution info to provide feedback about projects
        if (File.Exists(solutionPath))
        {
            var solutionInfo = SolutionFileParser.GetSolutionInfo(solutionPath);
            Console.WriteLine($"Found solution: {solutionInfo.Name}");
            
            // Parse all projects from solution (not just supported ones)
            var allProjects = GetAllProjectsFromSolution(solutionPath);
            var supportedProjects = solutionInfo.ProjectFiles; // These are the ones that will be processed
            
            Console.WriteLine($"Discovered {allProjects.Count} project(s):");
            
            foreach (var projectFile in allProjects)
            {
                var fileName = Path.GetFileName(projectFile);
                if (supportedProjects.Contains(projectFile))
                {
                    Console.WriteLine($"  ✓ {fileName} - will be processed");
                }
                else
                {
                    var extension = Path.GetExtension(projectFile);
                    Console.WriteLine($"  ⚠ {fileName} - unsupported project type ({extension})");
                }
            }
            
            var unsupportedCount = allProjects.Count - supportedProjects.Count;
            if (unsupportedCount > 0)
            {
                Console.WriteLine($"Warning: {unsupportedCount} project(s) will be skipped due to unsupported file types.");
                Console.WriteLine("Supported project types: .csproj, .vbproj, .fsproj");
            }
            
            if (supportedProjects.Any())
            {
                Console.WriteLine($"Processing {supportedProjects.Count} supported project(s)...");
            }
            else
            {
                Console.WriteLine("Warning: No supported projects found in solution.");
            }
        }
        
        return await fileProcessor.ProcessSolutionAsync(solutionPath, cancellationToken);
    }

    /// <summary>
    /// Gets all project references from a solution file, including unsupported ones
    /// </summary>
    private List<string> GetAllProjectsFromSolution(string solutionPath)
    {
        var solutionDir = Path.GetDirectoryName(solutionPath) ?? "";
        var projectPaths = new List<string>();
        
        if (!File.Exists(solutionPath))
            return projectPaths;
        
        var lines = File.ReadAllLines(solutionPath);
        
        foreach (var line in lines)
        {
            if (line.StartsWith("Project("))
            {
                // Parse project line format: Project("{GUID}") = "ProjectName", "RelativePath", "{ProjectGUID}"
                try
                {
                    // Find the equals sign and work from there
                    var equalsIndex = line.IndexOf(" = ");
                    if (equalsIndex >= 0)
                    {
                        var afterEquals = line.Substring(equalsIndex + 3);
                        
                        // Split by comma to get the three parts
                        var parts = afterEquals.Split(',').Select(p => p.Trim()).ToArray();
                        if (parts.Length >= 2)
                        {
                            // The second part is the relative path, remove quotes
                            var relativePath = parts[1].Trim('"', ' ');
                            if (!string.IsNullOrWhiteSpace(relativePath) && Path.HasExtension(relativePath))
                            {
                                var absolutePath = Path.GetFullPath(Path.Combine(solutionDir, relativePath));
                                projectPaths.Add(absolutePath);
                            }
                        }
                    }
                }
                catch
                {
                    // Skip malformed project lines
                }
            }
        }
        
        return projectPaths;
    }

    public ASTNode AnalyzeNode(SyntaxNode node)
    {
        // Get C# analyzer from registry for individual node analysis
        var analyzer = AnalyzerRegistry.GetAnalyzerByTypeName("CSharpSyntaxAnalyzer");
        return analyzer.AnalyzeNode(node);
    }

    public async Task WriteOutputAsync(ASTAnalysis analysis, string outputPath)
    {
        await _outputManager.WriteAsync(analysis, outputPath);
    }

    // Additional legacy methods used by integration tests
    public async Task<ASTAnalysis?> GenerateFromFileAsync(string filePath)
    {
        return await ProcessFileAsync(filePath);
    }

    public async Task<ProjectAnalysis?> GenerateFromProjectAsync(string projectPath)
    {
        ASTAnalysis? analysis = null;

        // Only handle specific project files, not directories
        if (File.Exists(projectPath) && IsProjectFile(projectPath))
        {
            analysis = await ProcessProjectAsync(projectPath);
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
        // Create the appropriate analyzer based on file type
        var analyzer = AnalyzerRegistry.GetAnalyzerForFile(filePath);
        return analyzer.AnalyzeSyntaxTree(root, filePath);
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
            // No longer need to dispose file processors since they're created on-demand with using statements
            _disposed = true;
        }
    }
}
