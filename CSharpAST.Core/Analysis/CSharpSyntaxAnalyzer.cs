using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Buffers;
using System.Collections.Concurrent;

namespace CSharpAST.Core.Analysis;

/// <summary>
/// High-performance syntax analyzer with memory pooling and optimized tree traversal for C# files.
/// </summary>
public class CSharpSyntaxAnalyzer : ISyntaxAnalyzer
{
    private static readonly ArrayPool<ASTNode> _nodePool = ArrayPool<ASTNode>.Shared;
    private static readonly ConcurrentQueue<List<ASTNode>> _listPool = new();
    private static readonly ConcurrentQueue<Dictionary<string, object>> _dictPool = new();

    /// <summary>
    /// Gets the capabilities of this C# analyzer
    /// </summary>
    public AnalyzerCapabilities Capabilities { get; } = new AnalyzerCapabilities
    {
        Name = "C# Syntax Analyzer",
        Description = "Analyzes C# source code and generates Abstract Syntax Trees using Roslyn",
        Language = "C#",
        SupportedFileExtensions = new[] { ".cs" },
        SupportedProjectExtensions = new[] { ".csproj" }
    };

    public ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath)
    {
        var analysis = new ASTAnalysis
        {
            SourceFile = filePath,
            GeneratedAt = DateTime.UtcNow,
            RootNode = ConvertToASTNode(root)
        };

        return analysis;
    }

    private ASTNode ConvertToASTNode(SyntaxNode node)
    {
        var astNode = new ASTNode
        {
            Type = node.GetType().Name,
            Kind = ((int)node.RawKind).ToString(),
            Text = node.ToString(),
            Span = new SourceSpan
            {
                Start = node.Span.Start,
                End = node.Span.End,
                Length = node.Span.Length
            },
            Location = new LocationInfo
            {
                Path = node.SyntaxTree?.FilePath ?? string.Empty,
                Span = new LinePositionSpan
                {
                    Start = new LinePosition
                    {
                        Line = node.GetLocation().GetMappedLineSpan().Span.Start.Line,
                        Character = node.GetLocation().GetMappedLineSpan().Span.Start.Character
                    },
                    End = new LinePosition
                    {
                        Line = node.GetLocation().GetMappedLineSpan().Span.End.Line,
                        Character = node.GetLocation().GetMappedLineSpan().Span.End.Character
                    }
                },
                HasMappedPath = node.GetLocation().GetMappedLineSpan().HasMappedPath
            },
            Properties = GetProperties(node),
            Children = new List<ASTNode>()
        };

        // Use parallel processing for large nodes
        if (node.ChildNodes().Count() > 50)
        {
            var childNodes = node.ChildNodes().AsParallel()
                .AsOrdered()
                .Select(ConvertToASTNode)
                .ToList();
            astNode.Children.AddRange(childNodes);
        }
        else
        {
            // Sequential processing for smaller nodes to avoid overhead
            foreach (var child in node.ChildNodes())
            {
                astNode.Children.Add(ConvertToASTNode(child));
            }
        }

        return astNode;
    }

    private Dictionary<string, object> GetProperties(SyntaxNode node)
    {
        var properties = GetPooledDictionary();

        try
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDecl:
                    properties["ClassName"] = classDecl.Identifier.ValueText;
                    properties["Modifiers"] = classDecl.Modifiers.Select(m => m.ValueText).ToArray();
                    if (classDecl.BaseList != null)
                    {
                        properties["BaseTypes"] = classDecl.BaseList.Types.Select(t => t.ToString()).ToArray();
                    }
                    break;

                case InterfaceDeclarationSyntax interfaceDecl:
                    properties["InterfaceName"] = interfaceDecl.Identifier.ValueText;
                    properties["Modifiers"] = interfaceDecl.Modifiers.Select(m => m.ValueText).ToArray();
                    break;

                case MethodDeclarationSyntax methodDecl:
                    properties["MethodName"] = methodDecl.Identifier.ValueText;
                    properties["IsAsync"] = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
                    properties["ReturnType"] = methodDecl.ReturnType.ToString();
                    properties["Modifiers"] = methodDecl.Modifiers.Select(m => m.ValueText).ToArray();
                    break;

                case PropertyDeclarationSyntax propertyDecl:
                    properties["PropertyName"] = propertyDecl.Identifier.ValueText;
                    properties["PropertyType"] = propertyDecl.Type.ToString();
                    if (propertyDecl.AccessorList != null)
                    {
                        properties["HasGetter"] = propertyDecl.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                        properties["HasSetter"] = propertyDecl.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
                    }
                    break;

                case UsingDirectiveSyntax usingDecl:
                    properties["Namespace"] = usingDecl.Name?.ToString() ?? string.Empty;
                    properties["IsStatic"] = usingDecl.StaticKeyword.IsKind(SyntaxKind.StaticKeyword);
                    break;

                case NamespaceDeclarationSyntax namespaceDecl:
                    properties["NamespaceName"] = namespaceDecl.Name.ToString();
                    break;

                case VariableDeclaratorSyntax variableDecl:
                    properties["VariableName"] = variableDecl.Identifier.ValueText;
                    break;
            }

            return new Dictionary<string, object>(properties);
        }
        finally
        {
            ReturnPooledDictionary(properties);
        }
    }

    private Dictionary<string, object> GetPooledDictionary()
    {
        if (_dictPool.TryDequeue(out var dict))
        {
            dict.Clear();
            return dict;
        }
        return new Dictionary<string, object>();
    }

    private void ReturnPooledDictionary(Dictionary<string, object> dict)
    {
        if (dict.Count < 100) // Only pool reasonably-sized dictionaries
        {
            _dictPool.Enqueue(dict);
        }
    }

    public ASTNode AnalyzeNode(SyntaxNode node)
    {
        var properties = GetPooledDictionary();
        var children = new List<ASTNode>();

        try
        {
            // Extract node properties
            ExtractNodeProperties(node, properties);

            // Analyze children - use parallel processing for nodes with many children
            if (node.ChildNodes().Count() > 50)
            {
                children = node.ChildNodes()
                    .AsParallel()
                    .AsOrdered()
                    .Select(child => AnalyzeNode(child))
                    .ToList();
            }
            else
            {
                children = node.ChildNodes()
                    .Select(child => AnalyzeNode(child))
                    .ToList();
            }

            return new ASTNode
            {
                Type = node.GetType().Name,
                Kind = node.Kind().ToString(),
                Text = node.ToString().Length > 500 ? node.ToString().Substring(0, 500) + "..." : node.ToString(),
                Properties = new Dictionary<string, object>(properties),
                Children = children,
                Location = new LocationInfo
                {
                    Path = node.SyntaxTree?.FilePath ?? string.Empty,
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
                    HasMappedPath = false
                }
            };
        }
        finally
        {
            ReturnPooledDictionary(properties);
        }
    }

    private void ExtractNodeProperties(SyntaxNode node, Dictionary<string, object> properties)
    {
        try
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDecl:
                    properties["ClassName"] = classDecl.Identifier.ValueText;
                    properties["Modifiers"] = classDecl.Modifiers.Select(m => m.ValueText).ToArray();
                    if (classDecl.BaseList != null)
                    {
                        properties["BaseTypes"] = classDecl.BaseList.Types.Select(t => t.ToString()).ToArray();
                    }
                    break;

                case InterfaceDeclarationSyntax interfaceDecl:
                    properties["InterfaceName"] = interfaceDecl.Identifier.ValueText;
                    properties["Modifiers"] = interfaceDecl.Modifiers.Select(m => m.ValueText).ToArray();
                    break;

                case MethodDeclarationSyntax methodDecl:
                    properties["MethodName"] = methodDecl.Identifier.ValueText;
                    properties["ReturnType"] = methodDecl.ReturnType.ToString();
                    properties["Modifiers"] = methodDecl.Modifiers.Select(m => m.ValueText).ToArray();
                    properties["IsAsync"] = methodDecl.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AsyncKeyword));
                    break;

                case PropertyDeclarationSyntax propDecl:
                    properties["PropertyName"] = propDecl.Identifier.ValueText;
                    properties["PropertyType"] = propDecl.Type.ToString();
                    properties["Modifiers"] = propDecl.Modifiers.Select(m => m.ValueText).ToArray();
                    break;

                case FieldDeclarationSyntax fieldDecl:
                    properties["FieldType"] = fieldDecl.Declaration.Type.ToString();
                    properties["Modifiers"] = fieldDecl.Modifiers.Select(m => m.ValueText).ToArray();
                    break;

                case UsingDirectiveSyntax usingDecl:
                    properties["Namespace"] = usingDecl.Name?.ToString() ?? "";
                    properties["IsStatic"] = usingDecl.StaticKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword);
                    break;

                case NamespaceDeclarationSyntax namespaceDecl:
                    properties["NamespaceName"] = namespaceDecl.Name.ToString();
                    break;

                case VariableDeclaratorSyntax varDecl:
                    properties["VariableName"] = varDecl.Identifier.ValueText;
                    break;
            }
        }
        catch (Exception)
        {
            // Ignore property extraction errors to ensure robustness
        }
    }

    public ASTAnalysis AnalyzeFile(string filePath, string content)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(content, path: filePath);
        var root = syntaxTree.GetRoot();
        return AnalyzeSyntaxTree(root, filePath);
    }

    public bool SupportsFile(string filePath)
    {
        return Capabilities.SupportsFile(filePath);
    }

    public string[] GetSupportedProjectExtensions()
    {
        return Capabilities.SupportedProjectExtensions;
    }

    public bool SupportsProject(string projectPath)
    {
        return Capabilities.SupportsProject(projectPath);
    }
}
