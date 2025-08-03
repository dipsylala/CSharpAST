namespace CSharpAST.Core.Processing;

/// <summary>
/// Interface for processing different types of files in a C# project with concurrent support.
/// </summary>
public interface IFileProcessor : IDisposable
{
    /// <summary>
    /// Processes a single .cs file and returns its AST analysis
    /// </summary>
    Task<ASTAnalysis?> ProcessFileAsync(string filePath);

    /// <summary>
    /// Process a single C# file asynchronously (optimized method name).
    /// </summary>
    Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a .csproj file and returns project information with AST analysis for included files
    /// </summary>
    Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a .sln file and returns solution information with AST analysis for all projects
    /// </summary>
    Task<ASTAnalysis?> ProcessSolutionAsync(string solutionPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process multiple C# files concurrently.
    /// </summary>
    Task<List<ASTAnalysis>> ProcessMultipleFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a file is supported for processing
    /// </summary>
    bool IsFileSupported(string filePath);
}
