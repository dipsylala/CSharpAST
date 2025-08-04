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

}
