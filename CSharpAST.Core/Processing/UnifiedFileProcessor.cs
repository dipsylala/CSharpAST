using CSharpAST.Core.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace CSharpAST.Core.Processing
{
    /// <summary>
    /// Enhanced file processor that handles C#, VB.NET, and Razor/CSHTML files.
    /// </summary>
    public class UnifiedFileProcessor : IFileProcessor
    {
        private readonly List<ISyntaxAnalyzer> _analyzers;

        public UnifiedFileProcessor(ISyntaxAnalyzer csharpAnalyzer)
        {
            _analyzers = new List<ISyntaxAnalyzer>
            {
                csharpAnalyzer,
                new VBSyntaxAnalyzer(),
                new RazorSyntaxAnalyzer()
            };
        }

        public UnifiedFileProcessor(IEnumerable<ISyntaxAnalyzer> analyzers)
        {
            _analyzers = analyzers.ToList();
        }

        public bool IsFileSupported(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            // Support individual source files via analyzers
            if (_analyzers.Any(analyzer => analyzer.SupportsFile(filePath)))
                return true;
                
            // Support project files that any analyzer is responsible for
            if (_analyzers.Any(analyzer => analyzer.SupportsProject(filePath)))
                return true;
                
            // Support solution files for aggregation
            return extension == ".sln";
        }

        public bool IsProjectSupported(string projectPath)
        {
            return _analyzers.Any(analyzer => analyzer.SupportsProject(projectPath));
        }

        public async Task<List<ASTAnalysis>> ProcessFilesAsync(IEnumerable<string> filePaths)
        {
            var tasks = filePaths.Select(ProcessFileAsync);
            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null).ToList()!;
        }

        public async Task<ASTAnalysis?> ProcessFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Warning: File not found: {filePath}");
                    return null;
                }

                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                
                // Handle solution files
                if (extension == ".sln")
                {
                    return await ProcessSolutionAsync(filePath);
                }
                
                // Handle project files based on analyzer responsibility
                if (_analyzers.Any(analyzer => analyzer.SupportsProject(filePath)))
                {
                    return await ProcessProjectAsync(filePath);
                }
                
                // Handle source files
                return await ProcessSourceFileAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                return CreateErrorAnalysis(filePath, ex);
            }
        }

        private async Task<ASTAnalysis?> ProcessSourceFileAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            
            // Find the appropriate analyzer for this file type
            var analyzer = _analyzers.FirstOrDefault(a => a.SupportsFile(filePath));
            
            if (analyzer != null)
            {
                return await Task.Run(() => analyzer.AnalyzeFile(filePath, content));
            }
            else
            {
                Console.WriteLine($"Warning: Unsupported file type: {filePath}");
                return CreateUnsupportedFileAnalysis(filePath);
            }
        }

        private static ASTAnalysis CreateUnsupportedFileAnalysis(string filePath)
        {
            return new ASTAnalysis
            {
                SourceFile = filePath,
                GeneratedAt = DateTime.UtcNow,
                RootNode = new ASTNode
                {
                    Type = "UnsupportedFile",
                    Kind = "UnsupportedFileType",
                    Text = $"Unsupported file type: {Path.GetExtension(filePath)}",
                    Properties = new Dictionary<string, object>
                    {
                        { "FileName", Path.GetFileName(filePath) },
                        { "Extension", Path.GetExtension(filePath) },
                        { "Reason", "File type not supported by the AST generator" }
                    }
                }
            };
        }

        public async Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return await ProcessFileAsync(filePath);
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
                        ["SupportedExtensions"] = GetSupportedExtensions(),
                        ["ProjectType"] = Path.GetExtension(projectPath),
                        ["ParsedFromProjectFile"] = true
                    },
                    Children = new List<ASTNode>()
                }
            };

            // Process each included file
            foreach (var sourceFile in includedFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var fileAnalysis = await ProcessSourceFileAsync(sourceFile);
                    if (fileAnalysis?.RootNode != null)
                    {
                        analysis.RootNode.Children.Add(fileAnalysis.RootNode);
                    }
                }
                catch (Exception ex)
                {
                    // Add error node for failed files
                    analysis.RootNode.Children.Add(new ASTNode
                    {
                        Type = "ErrorNode",
                        Kind = "Error",
                        Text = $"Failed to process {sourceFile}: {ex.Message}",
                        Properties = new Dictionary<string, object>
                        {
                            ["FilePath"] = sourceFile,
                            ["Error"] = ex.Message
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

            // Process each project sequentially
            foreach (var projectFile in projectFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var projectAnalysis = await ProcessProjectAsync(projectFile, cancellationToken);
                    if (projectAnalysis?.RootNode != null)
                    {
                        analysis.RootNode.Children.Add(projectAnalysis.RootNode);
                    }
                }
                catch (Exception ex)
                {
                    // Add error node for failed projects
                    analysis.RootNode.Children.Add(new ASTNode
                    {
                        Type = "ErrorNode",
                        Kind = "Error",
                        Text = $"Failed to process project {Path.GetFileName(projectFile)}: {ex.Message}",
                        Properties = new Dictionary<string, object>
                        {
                            ["ProjectPath"] = projectFile,
                            ["Error"] = ex.Message
                        },
                        Children = new List<ASTNode>()
                    });
                }
            }

            return analysis;
        }

        public async Task<List<ASTAnalysis>> ProcessMultipleFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
        {
            var tasks = filePaths.Select(async path => await ProcessFileAsync(path));
            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null).ToList()!;
        }

        public void Dispose()
        {
            // Nothing to dispose currently, but implementing for interface compliance
        }

        private string GetSupportedExtensions()
        {
            // Dynamically determine supported extensions by testing each analyzer
            var commonExtensions = new[] { ".cs", ".vb", ".cshtml", ".razor", ".fs", ".tsx", ".ts", ".jsx", ".js" };
            var supportedExtensions = new List<string>();
            
            foreach (var ext in commonExtensions)
            {
                if (_analyzers.Any(analyzer => analyzer.SupportsFile($"test{ext}")))
                {
                    supportedExtensions.Add(ext);
                }
            }
            
            return string.Join(", ", supportedExtensions);
        }

        private List<string> GetProjectFiles(string directoryPath)
        {
            // Find all common project file types
            var projectExtensions = new[] { "*.csproj", "*.vbproj", "*.fsproj", "*.proj" };
            var projectFiles = new List<string>();
            
            foreach (var pattern in projectExtensions)
            {
                projectFiles.AddRange(Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories));
            }
            
            return projectFiles;
        }

        private static ASTAnalysis CreateErrorAnalysis(string filePath, Exception ex)
        {
            return new ASTAnalysis
            {
                SourceFile = filePath,
                GeneratedAt = DateTime.UtcNow,
                RootNode = new ASTNode
                {
                    Type = "ProcessingError",
                    Kind = "FileProcessingError",
                    Text = $"Error processing file: {ex.Message}",
                    Properties = new Dictionary<string, object>
                    {
                        { "FileName", Path.GetFileName(filePath) },
                        { "ErrorMessage", ex.Message },
                        { "ErrorType", ex.GetType().Name }
                    }
                }
            };
        }
    }
}
