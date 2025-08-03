using CSharpAST.Core.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpAST.Core.Processing
{
    /// <summary>
    /// Enhanced file processor that handles both C# and Razor/CSHTML files.
    /// </summary>
    public class UnifiedFileProcessor : IFileProcessor
    {
        private readonly ISyntaxAnalyzer _csharpAnalyzer;
        private readonly RazorSyntaxAnalyzer _razorAnalyzer;

        public UnifiedFileProcessor(ISyntaxAnalyzer csharpAnalyzer)
        {
            _csharpAnalyzer = csharpAnalyzer;
            _razorAnalyzer = new RazorSyntaxAnalyzer();
        }

        public bool IsFileSupported(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".cs" || extension == ".cshtml" || extension == ".razor";
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

                var content = await File.ReadAllTextAsync(filePath);
                
                // Determine file type and process accordingly
                if (RazorSyntaxAnalyzer.IsRazorFile(filePath))
                {
                    return await ProcessRazorFileAsync(filePath, content);
                }
                else if (IsCSharpFile(filePath))
                {
                    return await ProcessCSharpFileAsync(filePath, content);
                }
                else
                {
                    Console.WriteLine($"Warning: Unsupported file type: {filePath}");
                    return CreateUnsupportedFileAnalysis(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                return CreateErrorAnalysis(filePath, ex);
            }
        }

        private async Task<ASTAnalysis> ProcessRazorFileAsync(string filePath, string content)
        {
            await Task.Yield(); // Make async for consistency
            
            var razorAst = _razorAnalyzer.AnalyzeRazorFile(filePath, content);
            
            return new ASTAnalysis
            {
                SourceFile = filePath,
                GeneratedAt = DateTime.UtcNow,
                RootNode = razorAst
            };
        }

        private async Task<ASTAnalysis> ProcessCSharpFileAsync(string filePath, string content)
        {
            await Task.Yield(); // Make async for consistency
            
            var syntaxTree = CSharpSyntaxTree.ParseText(content, path: filePath);
            var root = syntaxTree.GetRoot();
            
            return _csharpAnalyzer.AnalyzeSyntaxTree(root, filePath);
        }

        private static bool IsCSharpFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".cs";
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
            // For now, delegate to the standard file processor behavior
            // This could be enhanced later to specifically handle project files
            return await ProcessFileAsync(projectPath);
        }

        public async Task<ASTAnalysis?> ProcessSolutionAsync(string solutionPath, CancellationToken cancellationToken = default)
        {
            // For now, delegate to the standard file processor behavior
            // This could be enhanced later to specifically handle solution files
            return await ProcessFileAsync(solutionPath);
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
