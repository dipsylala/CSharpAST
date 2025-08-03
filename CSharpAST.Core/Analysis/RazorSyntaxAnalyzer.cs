using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CSharpAST.Core.Analysis;
using System.Text.RegularExpressions;

namespace CSharpAST.Core.Analysis
{
    /// <summary>
    /// Analyzes Razor/CSHTML files to extract both Razor syntax and embedded C# code.
    /// Uses a simplified approach due to Razor API access limitations.
    /// </summary>
    public class RazorSyntaxAnalyzer : ISyntaxAnalyzer
    {
        private readonly RazorProjectEngine _razorEngine;
        private static readonly Regex CSharpCodePattern = new Regex(@"@\{([^}]*)\}", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex CSharpExpressionPattern = new Regex(@"@([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)", RegexOptions.Compiled);

        public RazorSyntaxAnalyzer()
        {
            // Create a basic Razor project engine for parsing
            var fileSystem = RazorProjectFileSystem.Create(".");
            var razorEngine = RazorProjectEngine.Create(RazorConfiguration.Default, fileSystem, builder =>
            {
                builder.SetNamespace("Generated");
                builder.ConfigureClass((document, @class) =>
                {
                    @class.ClassName = "GeneratedTemplate";
                });
            });
            
            _razorEngine = razorEngine;
        }

        /// <summary>
        /// Analyzes a Razor/CSHTML file and extracts both Razor syntax tree and embedded C# code.
        /// </summary>
        public ASTNode AnalyzeRazorFile(string filePath, string content)
        {
            try
            {
                // Create root Razor AST node without using complex Razor APIs
                var razorAst = new ASTNode
                {
                    Type = "RazorDocument",
                    Kind = "RazorFile",
                    Text = content.Length > 200 ? content.Substring(0, 200) + "..." : content,
                    Span = new SourceSpan { Start = 0, End = content.Length, Length = content.Length },
                    Location = new LocationInfo
                    {
                        Path = filePath,
                        Span = new LinePositionSpan
                        {
                            Start = new LinePosition { Line = 0, Character = 0 },
                            End = new LinePosition { Line = content.Split('\n').Length - 1, Character = 0 }
                        }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "FileName", Path.GetFileName(filePath) },
                        { "FileType", "Razor/CSHTML" },
                        { "ContentLength", content.Length },
                        { "Analysis", "Basic pattern matching (Razor APIs have limited public access)" }
                    },
                    Children = new List<ASTNode>()
                };

                // Extract C# code blocks and expressions using regex
                var csharpNodes = ExtractCSharpNodes(content, filePath);
                razorAst.Children.AddRange(csharpNodes);

                // Try to generate C# code if possible
                TryAddGeneratedCode(razorAst, content, filePath);

                return razorAst;
            }
            catch (Exception ex)
            {
                return new ASTNode
                {
                    Type = "RazorParseError",
                    Kind = "ParseError",
                    Text = $"Error parsing Razor file: {Path.GetFileName(filePath)}",
                    Properties = new Dictionary<string, object>
                    {
                        { "Error", ex.Message },
                        { "FilePath", filePath },
                        { "FileName", Path.GetFileName(filePath) }
                    }
                };
            }
        }

        /// <summary>
        /// Extracts C# code nodes from Razor content using regex patterns.
        /// </summary>
        private List<ASTNode> ExtractCSharpNodes(string content, string filePath)
        {
            var nodes = new List<ASTNode>();
            var lines = content.Split('\n');

            // Extract C# code blocks (@{ ... })
            var codeMatches = CSharpCodePattern.Matches(content);
            foreach (Match match in codeMatches)
            {
                var lineNumber = GetLineNumber(content, match.Index);
                nodes.Add(new ASTNode
                {
                    Type = "CSharpCodeBlock",
                    Kind = "CodeBlock",
                    Text = match.Value,
                    Span = new SourceSpan { Start = match.Index, End = match.Index + match.Length, Length = match.Length },
                    Location = new LocationInfo
                    {
                        Path = filePath,
                        Span = new LinePositionSpan
                        {
                            Start = new LinePosition { Line = lineNumber, Character = 0 },
                            End = new LinePosition { Line = lineNumber, Character = match.Length }
                        }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "CSharpCode", match.Groups[1].Value },
                        { "LineNumber", lineNumber + 1 }
                    }
                });
            }

            // Extract C# expressions (@Model, @property, etc.)
            var expressionMatches = CSharpExpressionPattern.Matches(content);
            foreach (Match match in expressionMatches)
            {
                var lineNumber = GetLineNumber(content, match.Index);
                nodes.Add(new ASTNode
                {
                    Type = "CSharpExpression",
                    Kind = "Expression",
                    Text = match.Value,
                    Span = new SourceSpan { Start = match.Index, End = match.Index + match.Length, Length = match.Length },
                    Location = new LocationInfo
                    {
                        Path = filePath,
                        Span = new LinePositionSpan
                        {
                            Start = new LinePosition { Line = lineNumber, Character = 0 },
                            End = new LinePosition { Line = lineNumber, Character = match.Length }
                        }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "Expression", match.Groups[1].Value },
                        { "LineNumber", lineNumber + 1 }
                    }
                });
            }

            return nodes;
        }

        /// <summary>
        /// Attempts to add generated C# code analysis if possible.
        /// </summary>
        private void TryAddGeneratedCode(ASTNode razorAst, string content, string filePath)
        {
            try
            {
                // Try to use Razor engine if available
                var sourceDocument = RazorSourceDocument.Create(content, filePath);
                var codeDocument = _razorEngine.ProcessDesignTime(sourceDocument, filePath, null, new List<Microsoft.AspNetCore.Razor.Language.TagHelperDescriptor>());
                
                var generatedCSharp = ExtractGeneratedCSharpCode(codeDocument);
                if (!string.IsNullOrEmpty(generatedCSharp))
                {
                    var generatedAst = AnalyzeGeneratedCSharp(generatedCSharp, filePath);
                    razorAst.Children.Add(generatedAst);
                }
            }
            catch
            {
                // If Razor engine fails, add a note that full generation wasn't possible
                razorAst.Children.Add(new ASTNode
                {
                    Type = "RazorGenerationNote",
                    Kind = "Note",
                    Text = "Full Razor code generation not available due to API limitations",
                    Properties = new Dictionary<string, object>
                    {
                        { "Note", "Using pattern-based C# extraction instead" },
                        { "Reason", "Razor Language Service APIs have limited public access" }
                    }
                });
            }
        }

        /// <summary>
        /// Extracts generated C# code from the Razor code document.
        /// </summary>
        private string ExtractGeneratedCSharpCode(RazorCodeDocument codeDocument)
        {
            try
            {
                var csharpDocument = codeDocument.GetCSharpDocument();
                return csharpDocument?.GeneratedCode ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the line number for a character position in the content.
        /// </summary>
        private int GetLineNumber(string content, int position)
        {
            return content.Take(position).Count(c => c == '\n');
        }

        /// <summary>
        /// Determines if a file is a Razor/CSHTML file based on its extension.
        /// </summary>
        public static bool IsRazorFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".cshtml" || extension == ".razor";
        }

        /// <summary>
        /// Analyzes the generated C# code from Razor compilation.
        /// </summary>
        private ASTNode AnalyzeGeneratedCSharp(string generatedCode, string filePath)
        {
            try
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
                var root = syntaxTree.GetRoot();
                var analyzer = new CSharpSyntaxAnalyzer();
                var analysis = analyzer.AnalyzeSyntaxTree(root, filePath + "_generated.cs");

                return new ASTNode
                {
                    Type = "GeneratedCSharpCode",
                    Kind = "Generated",
                    Text = generatedCode.Length > 500 ? generatedCode.Substring(0, 500) + "..." : generatedCode,
                    Properties = new Dictionary<string, object>
                    {
                        { "GeneratedFrom", "RazorEngine" },
                        { "OriginalFile", filePath },
                        { "CodeLength", generatedCode.Length }
                    },
                    Children = new List<ASTNode> { analysis.RootNode }
                };
            }
            catch (Exception ex)
            {
                return new ASTNode
                {
                    Type = "GeneratedCodeError",
                    Kind = "Error",
                    Text = "Failed to analyze generated C# code",
                    Properties = new Dictionary<string, object>
                    {
                        { "Error", ex.Message },
                        { "OriginalFile", filePath }
                    }
                };
            }
        }

        // ISyntaxAnalyzer interface implementations
        public ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath)
        {
            // For Razor files, we can't easily work with a pre-parsed syntax tree
            // since Razor has its own parsing requirements. This method is mainly 
            // for C# and VB.NET analyzers.
            throw new NotSupportedException("RazorSyntaxAnalyzer requires file content analysis. Use AnalyzeFile instead.");
        }

        public ASTAnalysis AnalyzeFile(string filePath, string content)
        {
            var rootNode = AnalyzeRazorFile(filePath, content);
            
            return new ASTAnalysis
            {
                SourceFile = filePath,
                GeneratedAt = DateTime.UtcNow,
                RootNode = rootNode
            };
        }

        public ASTNode AnalyzeNode(SyntaxNode node)
        {
            // For Razor files, we primarily work with text content rather than syntax nodes
            // This method is mainly for C# and VB.NET analyzers.
            throw new NotSupportedException("RazorSyntaxAnalyzer works with file content rather than individual syntax nodes.");
        }

        public bool SupportsFile(string filePath)
        {
            return IsRazorFile(filePath);
        }

        public string[] GetSupportedProjectExtensions()
        {
            // Razor files are typically part of web projects, which are usually C# projects
            // But Razor itself doesn't have its own project type
            return new string[0]; // Empty array - Razor files are included in other project types
        }

        public bool SupportsProject(string projectPath)
        {
            // Razor analyzer doesn't own any project file types
            return false;
        }
    }
}
