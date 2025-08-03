using Microsoft.CodeAnalysis;

namespace CSharpAST.Core.Analysis;

/// <summary>
/// Interface for analyzing syntax trees and generating AST representations for multiple language types
/// </summary>
public interface ISyntaxAnalyzer
{
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

    /// <summary>
    /// Determines if this analyzer supports the specified file type
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns>True if the file type is supported by this analyzer</returns>
    bool SupportsFile(string filePath);

    /// <summary>
    /// Gets the project file extensions that this analyzer is responsible for
    /// </summary>
    /// <returns>Array of project file extensions (e.g., [".csproj"] for C# analyzer)</returns>
    [Obsolete("Use SupportsProject(string projectPath) instead for consistency with SupportsFile method", false)]
    string[] GetSupportedProjectExtensions();

    /// <summary>
    /// Determines if a project file is supported by this analyzer.
    /// For example, CSharpSyntaxAnalyzer supports .csproj files, VBSyntaxAnalyzer supports .vbproj files.
    /// </summary>
    /// <param name="projectPath">The project file path to check</param>
    /// <returns>True if the project file type is supported by this analyzer</returns>
    bool SupportsProject(string projectPath);
}
