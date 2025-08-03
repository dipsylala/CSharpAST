using Microsoft.CodeAnalysis;

namespace CSharpAST.Core.Analysis;

/// <summary>
/// Interface for analyzing C# syntax trees and generating AST representations
/// </summary>
public interface ISyntaxAnalyzer
{
    /// <summary>
    /// Analyzes a syntax tree and generates an AST analysis
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="filePath">The path to the source file</param>
    /// <returns>AST analysis containing the tree structure</returns>
    ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath);

    /// <summary>
    /// Analyzes a single syntax node and its children
    /// </summary>
    /// <param name="node">The syntax node to analyze</param>
    /// <returns>AST node representation</returns>
    ASTNode AnalyzeNode(SyntaxNode node);
}
