using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CSharpAST.Core.Analysis;

/// <summary>
/// VB.NET syntax analyzer that processes Visual Basic syntax trees and generates AST representations
/// </summary>
public class VBSyntaxAnalyzer : ISyntaxAnalyzer
{
    /// <summary>
    /// Gets the capabilities of this VB.NET analyzer
    /// </summary>
    public AnalyzerCapabilities Capabilities { get; } = new AnalyzerCapabilities
    {
        Name = "VB.NET Syntax Analyzer",
        Description = "Analyzes VB.NET source code and generates Abstract Syntax Trees using Roslyn",
        Language = "VB.NET",
        SupportedFileExtensions = new[] { ".vb" },
        SupportedProjectExtensions = new[] { ".vbproj" }
    };

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
                }
            },
            Properties = ExtractVBProperties(node),
            Children = new List<ASTNode>()
        };

        // Process child nodes
        foreach (var child in node.ChildNodes())
        {
            astNode.Children.Add(AnalyzeNode(child));
        }

        return astNode;
    }

    private Dictionary<string, object> ExtractVBProperties(SyntaxNode node)
    {
        var properties = new Dictionary<string, object>();

        // Add common properties
        properties["NodeKind"] = node.Kind().ToString();
        properties["IsStructuredTrivia"] = node.IsStructuredTrivia;
        properties["HasLeadingTrivia"] = node.HasLeadingTrivia;
        properties["HasTrailingTrivia"] = node.HasTrailingTrivia;

        // VB.NET specific node analysis
        switch (node)
        {
            case ClassBlockSyntax classBlock:
                ExtractClassProperties(classBlock, properties);
                break;
            case ModuleBlockSyntax moduleBlock:
                ExtractModuleProperties(moduleBlock, properties);
                break;
            case InterfaceBlockSyntax interfaceBlock:
                ExtractInterfaceProperties(interfaceBlock, properties);
                break;
            case StructureBlockSyntax structureBlock:
                ExtractStructureProperties(structureBlock, properties);
                break;
            case EnumBlockSyntax enumBlock:
                ExtractEnumProperties(enumBlock, properties);
                break;
            case MethodBlockSyntax methodBlock:
                ExtractMethodProperties(methodBlock, properties);
                break;
            case MethodStatementSyntax methodStatement:
                ExtractMethodStatementProperties(methodStatement, properties);
                break;
            case SubNewStatementSyntax constructorStatement:
                ExtractConstructorProperties(constructorStatement, properties);
                break;
            case PropertyBlockSyntax propertyBlock:
                ExtractPropertyProperties(propertyBlock, properties);
                break;
            case FieldDeclarationSyntax fieldDecl:
                ExtractFieldProperties(fieldDecl, properties);
                break;
            case VariableDeclaratorSyntax varDecl:
                ExtractVariableProperties(varDecl, properties);
                break;
            case ParameterSyntax parameter:
                ExtractParameterProperties(parameter, properties);
                break;
            case ImportsStatementSyntax importsStmt:
                ExtractImportsProperties(importsStmt, properties);
                break;
            case NamespaceBlockSyntax namespaceBlock:
                ExtractNamespaceProperties(namespaceBlock, properties);
                break;
        }

        return properties;
    }

    private void ExtractClassProperties(ClassBlockSyntax classBlock, Dictionary<string, object> properties)
    {
        properties["ClassName"] = classBlock.ClassStatement.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", classBlock.ClassStatement.Modifiers.Select(m => m.ValueText));
        properties["IsPartial"] = classBlock.ClassStatement.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        
        if (classBlock.ClassStatement.TypeParameterList != null)
        {
            properties["TypeParameters"] = classBlock.ClassStatement.TypeParameterList.Parameters.Select(p => p.Identifier.ValueText).ToArray();
        }
        
        // Look for inherits and implements in the class block
        var inheritsStatements = classBlock.DescendantNodes().OfType<InheritsStatementSyntax>().ToList();
        var implementsStatements = classBlock.DescendantNodes().OfType<ImplementsStatementSyntax>().ToList();
        
        if (inheritsStatements.Any())
        {
            properties["BaseTypes"] = inheritsStatements.SelectMany(i => i.Types.Select(t => t.ToString())).ToArray();
        }
        
        if (implementsStatements.Any())
        {
            properties["ImplementedInterfaces"] = implementsStatements.SelectMany(i => i.Types.Select(t => t.ToString())).ToArray();
        }
    }

    private void ExtractModuleProperties(ModuleBlockSyntax moduleBlock, Dictionary<string, object> properties)
    {
        properties["ModuleName"] = moduleBlock.ModuleStatement.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", moduleBlock.ModuleStatement.Modifiers.Select(m => m.ValueText));
    }

    private void ExtractInterfaceProperties(InterfaceBlockSyntax interfaceBlock, Dictionary<string, object> properties)
    {
        properties["InterfaceName"] = interfaceBlock.InterfaceStatement.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", interfaceBlock.InterfaceStatement.Modifiers.Select(m => m.ValueText));
        
        if (interfaceBlock.InterfaceStatement.TypeParameterList != null)
        {
            properties["TypeParameters"] = interfaceBlock.InterfaceStatement.TypeParameterList.Parameters.Select(p => p.Identifier.ValueText).ToArray();
        }
    }

    private void ExtractStructureProperties(StructureBlockSyntax structureBlock, Dictionary<string, object> properties)
    {
        properties["StructureName"] = structureBlock.StructureStatement.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", structureBlock.StructureStatement.Modifiers.Select(m => m.ValueText));
    }

    private void ExtractEnumProperties(EnumBlockSyntax enumBlock, Dictionary<string, object> properties)
    {
        properties["EnumName"] = enumBlock.EnumStatement.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", enumBlock.EnumStatement.Modifiers.Select(m => m.ValueText));
        
        if (enumBlock.EnumStatement.UnderlyingType != null)
        {
            properties["UnderlyingType"] = enumBlock.EnumStatement.UnderlyingType.ToString();
        }
    }

    private void ExtractMethodProperties(MethodBlockSyntax methodBlock, Dictionary<string, object> properties)
    {
        properties["MethodName"] = methodBlock.SubOrFunctionStatement.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", methodBlock.SubOrFunctionStatement.Modifiers.Select(m => m.ValueText));
        properties["IsAsync"] = methodBlock.SubOrFunctionStatement.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        
        if (methodBlock.SubOrFunctionStatement.ParameterList != null)
        {
            properties["ParameterCount"] = methodBlock.SubOrFunctionStatement.ParameterList.Parameters.Count;
        }
        
        if (methodBlock.SubOrFunctionStatement.AsClause != null)
        {
            properties["ReturnType"] = methodBlock.SubOrFunctionStatement.AsClause.ToString();
        }
    }

    private void ExtractMethodStatementProperties(MethodStatementSyntax methodStatement, Dictionary<string, object> properties)
    {
        properties["MethodName"] = methodStatement.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", methodStatement.Modifiers.Select(m => m.ValueText));
        properties["IsAsync"] = methodStatement.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        
        if (methodStatement.ParameterList != null)
        {
            properties["ParameterCount"] = methodStatement.ParameterList.Parameters.Count;
        }
        
        if (methodStatement.AsClause != null)
        {
            properties["ReturnType"] = methodStatement.AsClause.ToString();
        }
    }

    private void ExtractConstructorProperties(SubNewStatementSyntax constructorStatement, Dictionary<string, object> properties)
    {
        properties["ConstructorName"] = "New";
        properties["Modifiers"] = string.Join(" ", constructorStatement.Modifiers.Select(m => m.ValueText));
        
        if (constructorStatement.ParameterList != null)
        {
            properties["ParameterCount"] = constructorStatement.ParameterList.Parameters.Count;
        }
    }

    private void ExtractPropertyProperties(PropertyBlockSyntax propertyBlock, Dictionary<string, object> properties)
    {
        properties["PropertyName"] = propertyBlock.PropertyStatement.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", propertyBlock.PropertyStatement.Modifiers.Select(m => m.ValueText));
        
        if (propertyBlock.PropertyStatement.AsClause != null)
        {
            properties["PropertyType"] = propertyBlock.PropertyStatement.AsClause.ToString();
        }
        
        properties["HasGetter"] = propertyBlock.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorBlock);
        properties["HasSetter"] = propertyBlock.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorBlock);
    }

    private void ExtractFieldProperties(FieldDeclarationSyntax fieldDecl, Dictionary<string, object> properties)
    {
        properties["Modifiers"] = string.Join(" ", fieldDecl.Modifiers.Select(m => m.ValueText));
        properties["FieldCount"] = fieldDecl.Declarators.Count;
        
        if (fieldDecl.Declarators.Count > 0)
        {
            properties["FieldNames"] = fieldDecl.Declarators.SelectMany(d => d.Names.Select(n => n.Identifier.ValueText)).ToArray();
        }
    }

    private void ExtractVariableProperties(VariableDeclaratorSyntax varDecl, Dictionary<string, object> properties)
    {
        properties["VariableNames"] = varDecl.Names.Select(n => n.Identifier.ValueText).ToArray();
        
        if (varDecl.AsClause != null)
        {
            properties["VariableType"] = varDecl.AsClause.ToString();
        }
        
        properties["HasInitializer"] = varDecl.Initializer != null;
    }

    private void ExtractParameterProperties(ParameterSyntax parameter, Dictionary<string, object> properties)
    {
        properties["ParameterName"] = parameter.Identifier.Identifier.ValueText;
        properties["Modifiers"] = string.Join(" ", parameter.Modifiers.Select(m => m.ValueText));
        
        if (parameter.AsClause != null)
        {
            properties["ParameterType"] = parameter.AsClause.ToString();
        }
        
        properties["HasDefault"] = parameter.Default != null;
        properties["IsByRef"] = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.ByRefKeyword));
        properties["IsByVal"] = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.ByValKeyword));
        properties["IsOptional"] = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.OptionalKeyword));
        properties["IsParamArray"] = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamArrayKeyword));
    }

    private void ExtractImportsProperties(ImportsStatementSyntax importsStmt, Dictionary<string, object> properties)
    {
        properties["ImportedNamespaces"] = importsStmt.ImportsClauses.Select(c => c.ToString()).ToArray();
    }

    private void ExtractNamespaceProperties(NamespaceBlockSyntax namespaceBlock, Dictionary<string, object> properties)
    {
        properties["NamespaceName"] = namespaceBlock.NamespaceStatement.Name.ToString();
    }

    public ASTAnalysis AnalyzeFile(string filePath, string content)
    {
        var syntaxTree = VisualBasicSyntaxTree.ParseText(content, path: filePath);
        var root = syntaxTree.GetRoot();
        return AnalyzeSyntaxTree(root, filePath);
    }
}
