using Newtonsoft.Json;
using System.Text;

namespace CSharpAST.Core.OutputManager;

/// <summary>
/// JSON implementation of IOutputManager for JSON output formatting.
/// </summary>
public class JsonOutputManager : IOutputManager
{
    private readonly JsonSerializerSettings _jsonSettings;

    public JsonOutputManager()
    {
        _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };
    }

    public async Task WriteAsync(ASTAnalysis analysis, string outputPath)
    {
        if (!CanWriteToPath(outputPath))
            throw new ArgumentException($"Cannot write to path '{outputPath}'", nameof(outputPath));

        var json = JsonConvert.SerializeObject(analysis, _jsonSettings);
        var fullPath = Path.ChangeExtension(outputPath, GetFileExtension());
        
        await File.WriteAllTextAsync(fullPath, json, Encoding.UTF8);
    }

    public async Task WriteAsync(ProjectAnalysis analysis, string outputPath)
    {
        if (!CanWriteToPath(outputPath))
            throw new ArgumentException($"Cannot write to path '{outputPath}'", nameof(outputPath));

        var json = JsonConvert.SerializeObject(analysis, _jsonSettings);
        var fullPath = Path.ChangeExtension(outputPath, GetFileExtension());
        
        await File.WriteAllTextAsync(fullPath, json, Encoding.UTF8);
    }

    public async Task WriteStructuredOutputAsync(ASTAnalysis analysis, string outputPath, string? basePath = null)
    {
        // If outputPath is already absolute, use it directly; otherwise combine with basePath
        var fullPath = Path.IsPathRooted(outputPath) ? outputPath : 
                      (basePath != null ? Path.Combine(basePath, outputPath) : outputPath);
        
        if (!CanWriteToPath(fullPath))
            throw new ArgumentException($"Cannot write to path '{fullPath}'", nameof(outputPath));

        var json = JsonConvert.SerializeObject(analysis, _jsonSettings);
        
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
        
        await File.WriteAllTextAsync(outputFilePath, json, Encoding.UTF8);
    }

    public string GetFileExtension()
    {
        return ".json";
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
}
