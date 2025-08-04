namespace CSharpAST.Core.OutputManager;

/// <summary>
/// Interface for output formatting and file writing.
/// Each implementation handles a specific output format.
/// </summary>
public interface IOutputManager
{
    /// <summary>
    /// Write analysis results to file
    /// </summary>
    Task WriteAsync(ASTAnalysis analysis, string outputPath);
    
    /// <summary>
    /// Write project analysis results to file
    /// </summary>
    Task WriteAsync(ProjectAnalysis analysis, string outputPath);

    /// <summary>
    /// Write structured output preserving directory structure
    /// </summary>
    Task WriteStructuredOutputAsync(ASTAnalysis analysis, string outputPath, string? basePath = null);

    /// <summary>
    /// Get file extension for this output format
    /// </summary>
    string GetFileExtension();

    /// <summary>
    /// Validate output path accessibility
    /// </summary>
    bool CanWriteToPath(string outputPath);
}
