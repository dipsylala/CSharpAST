namespace CSharpAST.Core.Processing;

/// <summary>
/// Interface for processing different types of files in multi-language projects with concurrent support.
/// Supports C#, VB.NET, and Razor files through analyzer-driven file detection.
/// </summary>
public interface IFileProcessor : IDisposable
{
    /// <summary>
    /// Processes a single source file (C#, VB.NET, Razor) and returns its AST analysis
    /// </summary>
    Task<ASTAnalysis?> ProcessFileAsync(string filePath);

    /// <summary>
    /// Process a single C# file asynchronously (legacy method for backward compatibility).
    /// </summary>
    Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a project file (.csproj, .vbproj) and returns project information with AST analysis for included files
    /// </summary>
    Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a .sln file and returns solution information with AST analysis for all projects
    /// </summary>
    Task<ASTAnalysis?> ProcessSolutionAsync(string solutionPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process multiple source files concurrently across supported languages.
    /// </summary>
    Task<List<ASTAnalysis>> ProcessMultipleFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a file is supported for processing based on analyzer capabilities
    /// </summary>
    bool IsFileSupported(string filePath);
    
    /// <summary>
    /// Determines if a project file is supported for processing based on analyzer capabilities
    /// </summary>
    bool IsProjectSupported(string projectPath);
}
