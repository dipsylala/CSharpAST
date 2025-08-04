namespace CSharpAST.Core.Analysis;

/// <summary>
/// Describes the capabilities of a syntax analyzer, including supported file types and project types
/// </summary>
public class AnalyzerCapabilities
{
    /// <summary>
    /// File extensions that this analyzer can process (e.g., ".cs", ".vb", ".cshtml")
    /// </summary>
    public string[] SupportedFileExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Project file extensions that this analyzer is responsible for (e.g., ".csproj", ".vbproj")
    /// </summary>
    public string[] SupportedProjectExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Human-readable name of the analyzer
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this analyzer does
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Language or technology this analyzer targets
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Checks if this analyzer supports a specific file extension
    /// </summary>
    /// <param name="extension">File extension (with or without leading dot)</param>
    /// <returns>True if the extension is supported</returns>
    public bool SupportsFileExtension(string extension)
    {
        var normalizedExtension = extension.StartsWith(".") ? extension : "." + extension;
        return SupportedFileExtensions.Contains(normalizedExtension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if this analyzer supports a specific project extension
    /// </summary>
    /// <param name="extension">Project extension (with or without leading dot)</param>
    /// <returns>True if the extension is supported</returns>
    public bool SupportsProjectExtension(string extension)
    {
        var normalizedExtension = extension.StartsWith(".") ? extension : "." + extension;
        return SupportedProjectExtensions.Contains(normalizedExtension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if this analyzer supports a specific file path
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>True if the file is supported</returns>
    public bool SupportsFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return SupportsFileExtension(extension);
    }

    /// <summary>
    /// Checks if this analyzer supports a specific project path
    /// </summary>
    /// <param name="projectPath">Path to the project file</param>
    /// <returns>True if the project is supported</returns>
    public bool SupportsProject(string projectPath)
    {
        var extension = Path.GetExtension(projectPath);
        return SupportsProjectExtension(extension);
    }
}
