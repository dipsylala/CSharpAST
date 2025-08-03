using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using CSharpAST.Core.Analysis;

namespace CSharpAST.Core.Processing;

/// <summary>
/// Handles processing of different file types (.cs, .csproj, .sln) for AST generation
/// </summary>
public class FileProcessor : IFileProcessor
{
    private readonly ISyntaxAnalyzer _syntaxAnalyzer;
    private readonly CSharpParseOptions _parseOptions;

    public FileProcessor(ISyntaxAnalyzer syntaxAnalyzer)
    {
        _syntaxAnalyzer = syntaxAnalyzer;
        _parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
    }

    public bool IsFileSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension is ".cs" or ".csproj" or ".sln";
    }

    public async Task<ASTAnalysis?> ProcessFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".cs" => await ProcessCSharpFileAsync(filePath),
            ".csproj" => await ProcessProjectAsync(filePath),
            ".sln" => await ProcessSolutionAsync(filePath),
            _ => null
        };
    }

    public async Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(projectPath))
            return null;

        var projectDir = Path.GetDirectoryName(projectPath);
        if (string.IsNullOrEmpty(projectDir))
            return null;

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

        // Process each C# file
        foreach (var csFile in csFiles)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var fileAnalysis = await ProcessCSharpFileAsync(csFile, cancellationToken);
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
                    Text = $"Failed to process {csFile}: {ex.Message}",
                    Properties = new Dictionary<string, object>
                    {
                        ["FilePath"] = csFile,
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

        var solutionDir = Path.GetDirectoryName(solutionPath);
        if (string.IsNullOrEmpty(solutionDir))
            return null;

        // Find all .csproj files in the solution directory
        var projectFiles = Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories)
            .ToList();

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
                    ["ProjectCount"] = projectFiles.Count
                },
                Children = new List<ASTNode>()
            }
        };

        // Process each project
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
                    Text = $"Failed to process project {projectFile}: {ex.Message}",
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

    public async Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var sourceText = SourceText.From(await File.ReadAllTextAsync(filePath, cancellationToken), Encoding.UTF8);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, _parseOptions, filePath);
            var root = await syntaxTree.GetRootAsync(cancellationToken);

            return _syntaxAnalyzer.AnalyzeSyntaxTree(root, filePath);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<ASTAnalysis>> ProcessMultipleFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
    {
        var results = new List<ASTAnalysis>();
        
        foreach (var filePath in filePaths)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var analysis = await ProcessCSharpFileAsync(filePath, cancellationToken);
            if (analysis != null)
            {
                results.Add(analysis);
            }
        }

        return results;
    }

    public void Dispose()
    {
        // Nothing to dispose in this implementation
    }
}
