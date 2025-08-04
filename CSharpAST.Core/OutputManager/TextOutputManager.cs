using System.Linq;
using System.Text;

namespace CSharpAST.Core.OutputManager;

/// <summary>
/// Plain text implementation of IOutputManager for readable text output formatting.
/// </summary>
public class TextOutputManager : IOutputManager
{
    public async Task WriteAsync(ASTAnalysis analysis, string outputPath)
    {
        if (!CanWriteToPath(outputPath))
            throw new ArgumentException($"Cannot write to path '{outputPath}'", nameof(outputPath));

        var textOutput = FormatAnalysisAsText(analysis);
        var fullPath = Path.ChangeExtension(outputPath, GetFileExtension());
        
        await File.WriteAllTextAsync(fullPath, textOutput, Encoding.UTF8);
    }

    public async Task WriteAsync(ProjectAnalysis analysis, string outputPath)
    {
        if (!CanWriteToPath(outputPath))
            throw new ArgumentException($"Cannot write to path '{outputPath}'", nameof(outputPath));

        var textOutput = FormatProjectAnalysisAsText(analysis);
        var fullPath = Path.ChangeExtension(outputPath, GetFileExtension());
        
        await File.WriteAllTextAsync(fullPath, textOutput, Encoding.UTF8);
    }

    public async Task WriteStructuredOutputAsync(ASTAnalysis analysis, string outputPath, string? basePath = null)
    {
        // If outputPath is already absolute, use it directly; otherwise combine with basePath
        var fullPath = Path.IsPathRooted(outputPath) ? outputPath : 
                      (basePath != null ? Path.Combine(basePath, outputPath) : outputPath);
        
        if (!CanWriteToPath(fullPath))
            throw new ArgumentException($"Cannot write to path '{fullPath}'", nameof(outputPath));

        var textOutput = FormatAnalysisAsText(analysis);
        
        // For project files, create a filename based on the analysis source
        string outputFilePath;
        if (Directory.Exists(fullPath))
        {
            // fullPath is a directory, create filename from source file
            var sourceFileName = Path.GetFileNameWithoutExtension(analysis.SourceFile) ?? "analysis";
            outputFilePath = Path.Combine(fullPath, $"{sourceFileName}{GetFileExtension()}");
        }
        else
        {
            // fullPath is a file path, just ensure it has the right extension
            outputFilePath = Path.ChangeExtension(fullPath, GetFileExtension());
        }
        
        await File.WriteAllTextAsync(outputFilePath, textOutput, Encoding.UTF8);
    }

    public string GetFileExtension()
    {
        return ".txt";
    }

    public bool CanWriteToPath(string outputPath)
    {
        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string FormatAnalysisAsText(ASTAnalysis analysis)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"AST Analysis Report");
        sb.AppendLine($"==================");
        sb.AppendLine($"Source File: {analysis.SourceFile}");
        sb.AppendLine($"Generated At: {analysis.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        if (analysis.RootNode != null)
        {
            sb.AppendLine("Syntax Tree:");
            sb.AppendLine("============");
            FormatNodeAsText(analysis.RootNode, sb, 0);
        }

        return sb.ToString();
    }

    private string FormatProjectAnalysisAsText(ProjectAnalysis analysis)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Project Analysis Report");
        sb.AppendLine($"======================");
        sb.AppendLine($"Project: {analysis.ProjectName}");
        sb.AppendLine($"Generated At: {analysis.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Files Analyzed: {analysis.Files?.Count ?? 0}");
        sb.AppendLine();

        if (analysis.Files?.Any() == true)
        {
            sb.AppendLine("File Analyses:");
            sb.AppendLine("==============");
            foreach (var fileAnalysis in analysis.Files)
            {
                sb.AppendLine($"- {fileAnalysis.SourceFile}");
            }
        }

        return sb.ToString();
    }

    private void FormatNodeAsText(ASTNode node, StringBuilder sb, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 2);
        sb.AppendLine($"{indent}{node.Type} ({node.Kind})");

        if (node.Properties?.Any() == true)
        {
            foreach (var prop in node.Properties)
            {
                sb.AppendLine($"{indent}  {prop.Key}: {prop.Value}");
            }
        }

        if (node.Children?.Any() == true)
        {
            foreach (var child in node.Children)
            {
                FormatNodeAsText(child, sb, indentLevel + 1);
            }
        }
    }
}
