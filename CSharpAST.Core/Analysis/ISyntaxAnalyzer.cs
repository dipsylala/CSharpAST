using Microsoft.CodeAnalysis;

namespace CSharpAST.Core.Analysis;

/// <summary>
/// Interface for analyzing syntax trees and generating AST representations for multiple language types
/// </summary>
public interface ISyntaxAnalyzer
{
    /// <summary>
    /// Gets the capabilities of this analyzer, including supported file types and project types
    /// </summary>
    AnalyzerCapabilities Capabilities { get; }

    /// <summary>
    /// Analyzes a syntax tree and generates an AST analysis (for C# and VB.NET files)
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="filePath">The path to the source file</param>
    /// <returns>AST analysis containing the tree structure</returns>
    ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath);

    /// <summary>
    /// Analyzes a file from content and generates an AST analysis (for all supported file types)
    /// </summary>
    /// <param name="filePath">The path to the source file</param>
    /// <param name="content">The file content</param>
    /// <returns>AST analysis containing the tree structure</returns>
    ASTAnalysis AnalyzeFile(string filePath, string content);

    /// <summary>
    /// Analyzes a single syntax node and its children
    /// </summary>
    /// <param name="node">The syntax node to analyze</param>
    /// <returns>AST node representation</returns>
    ASTNode AnalyzeNode(SyntaxNode node);

    // Legacy support methods - these will be replaced by Capabilities property
    /// <summary>
    /// Determines if this analyzer supports the specified file type
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns>True if the file type is supported by this analyzer</returns>
    [Obsolete("Use Capabilities.SupportsFile(filePath) instead")]
    bool SupportsFile(string filePath);

    /// <summary>
    /// Gets the project file extensions that this analyzer is responsible for
    /// </summary>
    /// <returns>Array of project file extensions (e.g., [".csproj"] for C# analyzer)</returns>
    [Obsolete("Use Capabilities.SupportedProjectExtensions instead")]
    string[] GetSupportedProjectExtensions();

    /// <summary>
    /// Determines if a project file is supported by this analyzer.
    /// For example, CSharpSyntaxAnalyzer supports .csproj files, VBSyntaxAnalyzer supports .vbproj files.
    /// </summary>
    /// <param name="projectPath">The project file path to check</param>
    /// <returns>True if the project file type is supported by this analyzer</returns>
    [Obsolete("Use Capabilities.SupportsProject(projectPath) instead")]
    bool SupportsProject(string projectPath);
}
