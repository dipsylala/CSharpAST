using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpAST.Core.Analysis;

/// <summary>
/// Core syntax analyzer that processes C# syntax trees and generates AST representations
/// </summary>
public class SyntaxAnalyzer : ISyntaxAnalyzer
{
    public ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath)
    {
        var analysis = new ASTAnalysis
        {
            SourceFile = filePath,
            GeneratedAt = DateTime.UtcNow,
            RootNode = AnalyzeNode(root)
        };

        return analysis;
    }

    public ASTNode AnalyzeNode(SyntaxNode node)
    {
        var astNode = new ASTNode
        {
            Type = node.GetType().Name,
            Kind = node.RawKind.ToString(),
            Text = node.ToString(),
            Span = new SourceSpan
            {
                Start = node.Span.Start,
                End = node.Span.End,
                Length = node.Span.Length
            },
            Location = new LocationInfo
            {
                Path = node.SyntaxTree?.FilePath ?? "",
                Span = new LinePositionSpan
                {
                    Start = new LinePosition
                    {
                        Line = node.GetLocation().GetLineSpan().StartLinePosition.Line,
                        Character = node.GetLocation().GetLineSpan().StartLinePosition.Character
                    },
                    End = new LinePosition
                    {
                        Line = node.GetLocation().GetLineSpan().EndLinePosition.Line,
                        Character = node.GetLocation().GetLineSpan().EndLinePosition.Character
                    }
                },
                HasMappedPath = node.SyntaxTree?.HasCompilationUnitRoot ?? false
            },
            Properties = ExtractNodeProperties(node),
            Children = new List<ASTNode>()
        };

        // Analyze child nodes
        foreach (var child in node.ChildNodes())
        {
            astNode.Children.Add(AnalyzeNode(child));
        }

        return astNode;
    }

    private Dictionary<string, object> ExtractNodeProperties(SyntaxNode node)
    {
        var properties = new Dictionary<string, object>();

        // Add node-specific properties based on type
        switch (node)
        {
            case ClassDeclarationSyntax classDecl:
                properties["ClassName"] = classDecl.Identifier.ValueText;
                properties["Modifiers"] = classDecl.Modifiers.Select(m => m.ValueText).ToList();
                properties["BaseTypes"] = classDecl.BaseList?.Types.Select(t => t.ToString()).ToList() ?? new List<string>();
                break;

            case MethodDeclarationSyntax methodDecl:
                properties["MethodName"] = methodDecl.Identifier.ValueText;
                properties["IsAsync"] = methodDecl.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AsyncKeyword));
                properties["ReturnType"] = methodDecl.ReturnType.ToString();
                properties["Modifiers"] = methodDecl.Modifiers.Select(m => m.ValueText).ToList();
                break;

            case PropertyDeclarationSyntax propDecl:
                properties["PropertyName"] = propDecl.Identifier.ValueText;
                properties["PropertyType"] = propDecl.Type.ToString();
                properties["HasGetter"] = propDecl.AccessorList?.Accessors.Any(a => a.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration)) ?? false;
                properties["HasSetter"] = propDecl.AccessorList?.Accessors.Any(a => a.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetAccessorDeclaration)) ?? false;
                break;

            case UsingDirectiveSyntax usingDecl:
                properties["Namespace"] = usingDecl.Name?.ToString() ?? "";
                properties["IsStatic"] = usingDecl.StaticKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword);
                break;

            case InterfaceDeclarationSyntax interfaceDecl:
                properties["InterfaceName"] = interfaceDecl.Identifier.ValueText;
                properties["Modifiers"] = interfaceDecl.Modifiers.Select(m => m.ValueText).ToList();
                break;
        }

        return properties;
    }
}
