using FluentAssertions;
using CSharpAST.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CSharpAST.IntegrationTests;

public class SyntaxPatternAnalysisTests : TestBase
{
    private readonly ILogger<SyntaxPatternAnalysisTests> _syntaxLogger;

    public SyntaxPatternAnalysisTests()
    {
        _syntaxLogger = _serviceProvider.GetRequiredService<ILogger<SyntaxPatternAnalysisTests>>();
    }

    [Fact]
    public async Task AnalyzeAsyncAwaitPatterns_ShouldCaptureAllVariations()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify various async patterns
        var asyncPatterns = new[]
        {
            "async Task<string>",
            "async Task<List<ProcessResult>>",
            "async Task<ProcessResult>",
            "await ValidateInputAsync",
            "await ApplyProcessorsAsync",
            "await ProcessAsync"
        };

        foreach (var pattern in asyncPatterns)
        {
            jsonOutput.Should().Contain(pattern, $"Should capture async pattern: {pattern}");
        }

        // Verify async method modifiers are captured
        jsonOutput.Should().Contain("\"IsAsync\": true");
        
        _logger.LogInformation("Successfully verified async/await patterns in AST");
    }

    [Fact]
    public async Task AnalyzeGenericConstraints_ShouldCaptureComplexGenerics()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify generic syntax nodes are captured in AST tree
        var genericSyntaxTypes = new[]
        {
            "GenericNameSyntax",
            "TypeParameterListSyntax",
            "TypeArgumentListSyntax"
        };

        foreach (var syntaxType in genericSyntaxTypes)
        {
            jsonOutput.Should().Contain(syntaxType, $"Should capture generic syntax type: {syntaxType}");
        }

        // Verify actual generic type usage in text
        jsonOutput.Should().Contain("Task<", "Should contain generic Task usage");
        
        _logger.LogInformation("Successfully verified generic type patterns in AST");
    }

    [Fact]
    public async Task AnalyzeLINQPatterns_ShouldCaptureQueryAndMethodSyntax()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify LINQ method syntax
        var methodSyntaxPatterns = new[]
        {
            "metadata.Where",
            "batch.Select",
            ".ToList()",
            "m => _processors.ContainsKey(m.Key)"
        };

        foreach (var pattern in methodSyntaxPatterns)
        {
            jsonOutput.Should().Contain(pattern, $"Should capture LINQ method syntax: {pattern}");
        }

        _logger.LogInformation("Successfully verified LINQ patterns in AST");
    }

    [Fact]
    public async Task AnalyzeExceptionHandling_ShouldCaptureComplexPatterns()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify exception handling patterns
        var exceptionPatterns = new[]
        {
            "try",
            "catch (Exception ex)",
            "throw new ArgumentNullException",
            "ArgumentNullException(nameof("
        };

        foreach (var pattern in exceptionPatterns)
        {
            jsonOutput.Should().Contain(pattern, $"Should capture exception pattern: {pattern}");
        }

        // Verify try-catch structure
        jsonOutput.Should().Contain("TryStatementSyntax");
        jsonOutput.Should().Contain("CatchClauseSyntax");
        jsonOutput.Should().Contain("ThrowStatementSyntax");

        _logger.LogInformation("Successfully verified exception handling patterns in AST");
    }

    [Fact]
    public async Task AnalyzeStringInterpolation_ShouldIdentifySecurityRisks()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "SecurityPatterns.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify string interpolation syntax nodes are captured in AST tree
        jsonOutput.Should().Contain("InterpolatedStringExpressionSyntax", "Should capture interpolated string syntax in AST tree");
        jsonOutput.Should().Contain("InterpolationSyntax", "Should capture interpolation syntax in AST tree");

        // Verify actual string interpolation content is preserved
        jsonOutput.Should().Contain("$\\\"<div>Hello {userInput}</div>\\\"", "Should contain string interpolation patterns");

        _logger.LogInformation("Successfully verified string interpolation patterns in AST");
    }

    [Fact]
    public async Task AnalyzePropertyPatterns_ShouldCaptureModernCSharpFeatures()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify property patterns
        var propertyPatterns = new[]
        {
            "public string Id { get; set; } = string.Empty",
            "public string Content { get; set; } = string.Empty",
            "public bool Success { get; set; }",
            "public string ProcessedContent { get; set; } = string.Empty"
        };

        foreach (var pattern in propertyPatterns)
        {
            jsonOutput.Should().Contain(pattern, $"Should capture property pattern: {pattern}");
        }

        // Verify property syntax elements
        jsonOutput.Should().Contain("PropertyDeclarationSyntax");
        jsonOutput.Should().Contain("AccessorDeclarationSyntax");

        _logger.LogInformation("Successfully verified property patterns in AST");
    }

    [Fact]
    public async Task AnalyzeUsingStatements_ShouldCaptureResourceManagement()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "SecurityPatterns.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify using statement patterns
        var usingPatterns = new[]
        {
            "using System;",
            "using System.Collections.Generic;",
            "using System.Security.Cryptography;",
            "using System.Text;"
        };

        foreach (var pattern in usingPatterns)
        {
            jsonOutput.Should().Contain(pattern, $"Should capture using pattern: {pattern}");
        }

        // Verify using syntax elements
        jsonOutput.Should().Contain("UsingDirectiveSyntax");

        _logger.LogInformation("Successfully verified using statement patterns in AST");
    }

    [Fact]
    public async Task AnalyzeComplexMethodSignatures_ShouldCaptureFullDetails()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify method signature components
        var signatureComponents = new[]
        {
            "MethodDeclarationSyntax",
            "ParameterListSyntax",
            "TypeParameterListSyntax",
            "ReturnType",
            "Modifiers"
        };

        foreach (var component in signatureComponents)
        {
            jsonOutput.Should().Contain(component, $"Should capture method signature component: {component}");
        }

        // Verify specific method details are captured
        jsonOutput.Should().Contain("\"MethodName\": \"ProcessAsync\"");
        jsonOutput.Should().Contain("\"MethodName\": \"ProcessBatchAsync\"");
        jsonOutput.Should().Contain("\"IsAsync\": true");

        _logger.LogInformation("Successfully verified complex method signature patterns in AST");
    }

    [Fact]
    public async Task AnalyzeClassHierarchy_ShouldCaptureInheritanceAndInterfaces()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify class hierarchy patterns
        var hierarchyPatterns = new[]
        {
            "ComplexAsyncExample : IDataProcessor",
            "interface IDataProcessor",
            "public class ComplexAsyncExample"
        };

        foreach (var pattern in hierarchyPatterns)
        {
            jsonOutput.Should().Contain(pattern, $"Should capture hierarchy pattern: {pattern}");
        }

        // Verify syntax elements for inheritance
        jsonOutput.Should().Contain("ClassDeclarationSyntax");
        jsonOutput.Should().Contain("InterfaceDeclarationSyntax");
        jsonOutput.Should().Contain("BaseListSyntax");

        _logger.LogInformation("Successfully verified class hierarchy patterns in AST");
    }

    [Fact]
    public async Task AnalyzeNamespaceStructure_ShouldCaptureOrganization()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "SecurityPatterns.cs");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        // Assert - Verify namespace patterns
        var namespacePatterns = new[]
        {
            "namespace SecurityTestExample",
            "NamespaceDeclarationSyntax",
            "QualifiedNameSyntax"
        };

        foreach (var pattern in namespacePatterns)
        {
            jsonOutput.Should().Contain(pattern, $"Should capture namespace pattern: {pattern}");
        }

        // Verify namespace organization
        jsonOutput.Should().Contain("CompilationUnitSyntax");
        jsonOutput.Should().Contain("UsingDirectiveSyntax");

        _logger.LogInformation("Successfully verified namespace structure patterns in AST");
    }
}
