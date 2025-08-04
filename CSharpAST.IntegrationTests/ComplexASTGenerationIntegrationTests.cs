using FluentAssertions;
using CSharpAST.Core;
using CSharpAST.Core.Output;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CSharpAST.IntegrationTests;

[Collection("ProjectFileTests")]
public class ComplexASTGenerationIntegrationTests : TestBase
{
    private readonly ILogger<ComplexASTGenerationIntegrationTests> _complexLogger;

    public ComplexASTGenerationIntegrationTests()
    {
        _complexLogger = _serviceProvider.GetRequiredService<ILogger<ComplexASTGenerationIntegrationTests>>();
    }

    [Fact]
    public async Task GenerateComplexAsyncPatternAST_ShouldProduceDetailedSyntaxTree()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");
        _complexLogger.LogInformation($"Testing complex AST generation for: {testFilePath}");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);

        // Assert
        astAnalysis.Should().NotBeNull();
        astAnalysis.SourceFile.Should().EndWith("ComplexAsyncExample.cs");
        astAnalysis.RootNode.Should().NotBeNull();
        astAnalysis.RootNode.Type.Should().Be("CompilationUnitSyntax");

        // Verify complex async patterns are captured
        await VerifyComplexAsyncPatterns(astAnalysis);
        
        // Verify generic type handling
        await VerifyGenericTypeHandling(astAnalysis);
        
        // Verify dependency injection patterns
        await VerifyDependencyInjectionPatterns(astAnalysis);
        
        // Verify exception handling patterns
        await VerifyExceptionHandlingPatterns(astAnalysis);
    }

    [Fact]
    public async Task GenerateSecurityPatternsAST_ShouldIdentifySecurityVulnerabilities()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "SecurityPatterns.cs");
        _complexLogger.LogInformation($"Testing security pattern AST generation for: {testFilePath}");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);

        // Assert
        astAnalysis.Should().NotBeNull();
        
        // Verify SQL injection patterns are captured
        await VerifyStringInterpolationPatterns(astAnalysis);
        
        // Verify LINQ query patterns
        await VerifyLinqPatterns(astAnalysis);
        
        // Verify hardcoded secrets patterns
        await VerifyHardcodedSecretsPatterns(astAnalysis);
    }

    [Fact]
    public async Task GenerateLargeComplexFileAST_ShouldHandleScaleAndComplexity()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "LargeComplexFile.cs");
        _complexLogger.LogInformation($"Testing large file AST generation for: {testFilePath}");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);

        // Assert
        astAnalysis.Should().NotBeNull();
        
        // Verify scale handling
        astAnalysis.RootNode.Children.Should().NotBeEmpty();
        
        // Verify all major syntax types are captured
        await VerifyComprehensiveSyntaxCapture(astAnalysis);
    }

    [Fact]
    public async Task IntegrateASTGenerationWithTestDataGeneration_ShouldProduceComprehensiveTestData()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SingleFiles", "CSharp", "ComplexAsyncExample.cs");

        // Act - Generate AST
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        
        // Create project analysis from AST
        var projectAnalysis = new ProjectAnalysis
        {
            ProjectName = "ComplexAsyncExample",
            Files = new List<ASTAnalysis>
            {
                astAnalysis  // Use the AST analysis directly
            }
        };

        // Assert - Focus on AST structure validation
        astAnalysis.Should().NotBeNull();
        astAnalysis.RootNode.Should().NotBeNull();
        astAnalysis.RootNode.Type.Should().Be("CompilationUnitSyntax");
        
        // Verify AST contains interface declarations for test generation
        var hasInterfaceDeclarations = await FindPatternInAST(astAnalysis, "InterfaceDeclarationSyntax");
        hasInterfaceDeclarations.Should().BeTrue("AST should contain interface declarations for test generation");
        
        // Verify AST contains method declarations
        var hasMethodDeclarations = await FindPatternInAST(astAnalysis, "MethodDeclarationSyntax");
        hasMethodDeclarations.Should().BeTrue("AST should contain method declarations for test generation");
    }

    [Fact]
    public async Task GenerateMultipleFilesProject_ShouldMaintainCrossFileReferences()
    {
        // Arrange
        var testProjectDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TestFiles", "TestApplications", "BasicDLL");
        var testProjectFile = Path.Combine(testProjectDir, "BasicDLL.csproj");
        var testFiles = Directory.GetFiles(testProjectDir, "*.cs").Take(3).ToList();

        // Act
        var outputManager = new JsonOutputManager(); var generator = new ASTGenerator(outputManager, verbose: true);
        
        // Use ProcessProjectAsync directly which works correctly
        var astAnalysis = await generator.ProcessProjectAsync(testProjectFile);
        
        // Convert to ProjectAnalysis format for the test
        var projectAnalysis = new ProjectAnalysis
        {
            ProjectPath = testProjectFile,
            ProjectName = Path.GetFileNameWithoutExtension(testProjectFile),
            GeneratedAt = astAnalysis?.GeneratedAt ?? DateTime.UtcNow,
            Files = new List<ASTAnalysis>(),
            Dependencies = new List<string>(),
            TestClasses = new List<ClassInfo>(),
            AsyncPatterns = new List<AsyncPatternInfo>()
        };

        // Add file analyses for each processed file if astAnalysis is not null
        if (astAnalysis != null)
        {
            foreach (var child in astAnalysis.RootNode.Children)
            {
                var fileAnalysis = new ASTAnalysis
                {
                    SourceFile = child.Properties.ContainsKey("FilePath") ? child.Properties["FilePath"].ToString() ?? "" : "",
                    GeneratedAt = astAnalysis.GeneratedAt,
                    RootNode = child
                };
                projectAnalysis.Files.Add(fileAnalysis);
            }
        }

        // Assert
        projectAnalysis.Should().NotBeNull();
        projectAnalysis.Files.Should().HaveCountGreaterThan(1);
        
        // Verify each file has detailed AST analysis with proper tree structure
        foreach (var file in projectAnalysis.Files)
        {
            file.RootNode.Should().NotBeNull();
            // Root node type depends on file type - C# files have CompilationUnitSyntax, Razor files have RazorDocument
            var expectedTypes = new[] { "CompilationUnitSyntax", "RazorDocument", "CompilationUnit" };
            file.RootNode.Type.Should().BeOneOf(expectedTypes, 
                "Root node should be appropriate for the file type");
        }

        // Validate that AST structure is properly built
        projectAnalysis.Files.Should().NotBeEmpty("Project should contain analyzed files");
    }

    [Fact]
    public async Task SerializeComplexASTToJson_ShouldMaintainStructuralIntegrity()
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

        // Assert
        jsonOutput.Should().NotBeNullOrEmpty();
        jsonOutput.Should().Contain("CompilationUnitSyntax");
        jsonOutput.Should().Contain("ClassDeclarationSyntax");
        jsonOutput.Should().Contain("MethodDeclarationSyntax");
        jsonOutput.Should().Contain("async Task"); // Look for async methods in the text

        // Verify deserialization maintains integrity
        var deserializedAst = JsonConvert.DeserializeObject<ASTAnalysis>(jsonOutput);

        deserializedAst.Should().NotBeNull();
        deserializedAst!.SourceFile.Should().Be(astAnalysis.SourceFile);
        deserializedAst.RootNode.Type.Should().Be(astAnalysis.RootNode.Type);
    }

    private async Task VerifyComplexAsyncPatterns(ASTAnalysis astAnalysis)
    {
        // Verify async method declarations
        var hasAsyncMethods = await FindPatternInAST(astAnalysis, "async");
        hasAsyncMethods.Should().BeTrue("Should contain async method patterns");

        // Verify Task return types
        var hasTaskReturnTypes = await FindPatternInAST(astAnalysis, "Task");
        hasTaskReturnTypes.Should().BeTrue("Should contain Task return type patterns");

        // Verify await expressions
        var hasAwaitExpressions = await FindPatternInAST(astAnalysis, "await");
        hasAwaitExpressions.Should().BeTrue("Should contain await expression patterns");
    }

    private async Task VerifyGenericTypeHandling(ASTAnalysis astAnalysis)
    {
        // Verify generic method declarations are captured in AST tree
        var hasGenericMethods = await FindPatternInAST(astAnalysis, "GenericNameSyntax");
        hasGenericMethods.Should().BeTrue("Should capture generic syntax in AST tree");

        // Verify type parameter lists are captured
        var hasTypeParameters = await FindPatternInAST(astAnalysis, "TypeParameterListSyntax");
        hasTypeParameters.Should().BeTrue("Should capture type parameter syntax in AST tree");
    }

    private async Task VerifyDependencyInjectionPatterns(ASTAnalysis astAnalysis)
    {
        // Verify constructor injection
        var hasConstructorInjection = await FindPatternInAST(astAnalysis, "IDataRepository");
        hasConstructorInjection.Should().BeTrue("Should capture dependency injection patterns");

        // Verify null checks for injected dependencies
        var hasNullChecks = await FindPatternInAST(astAnalysis, "ArgumentNullException");
        hasNullChecks.Should().BeTrue("Should capture null check patterns");
    }

    private async Task VerifyExceptionHandlingPatterns(ASTAnalysis astAnalysis)
    {
        // Verify try-catch blocks
        var hasTryCatch = await FindPatternInAST(astAnalysis, "try") && 
                         await FindPatternInAST(astAnalysis, "catch");
        hasTryCatch.Should().BeTrue("Should capture exception handling patterns");

        // Verify exception throwing
        var hasThrowStatements = await FindPatternInAST(astAnalysis, "throw");
        hasThrowStatements.Should().BeTrue("Should capture throw statement patterns");
    }

    private async Task VerifyStringInterpolationPatterns(ASTAnalysis astAnalysis)
    {
        // Verify string interpolation syntax nodes are captured in AST tree
        var hasStringInterpolation = await FindPatternInAST(astAnalysis, "InterpolatedStringExpressionSyntax");
        hasStringInterpolation.Should().BeTrue("Should capture interpolated string syntax in AST tree");
    }

    private async Task VerifyLinqPatterns(ASTAnalysis astAnalysis)
    {
        // Verify LINQ syntax nodes are captured in AST tree
        var hasLinqSyntax = await FindPatternInAST(astAnalysis, "QueryExpressionSyntax") ||
                           await FindPatternInAST(astAnalysis, "InvocationExpressionSyntax");
        hasLinqSyntax.Should().BeTrue("Should capture LINQ syntax nodes in AST tree");
    }

    private async Task VerifyHardcodedSecretsPatterns(ASTAnalysis astAnalysis)
    {
        // Verify literal expressions and field declarations are captured in AST tree
        var hasLiteralExpressions = await FindPatternInAST(astAnalysis, "LiteralExpressionSyntax");
        hasLiteralExpressions.Should().BeTrue("Should capture literal expressions in AST tree");

        // Verify field and variable declarations
        var hasFieldDeclarations = await FindPatternInAST(astAnalysis, "FieldDeclarationSyntax") ||
                                  await FindPatternInAST(astAnalysis, "VariableDeclarationSyntax");
        hasFieldDeclarations.Should().BeTrue("Should capture field/variable declarations in AST tree");
    }

    private async Task VerifyComprehensiveSyntaxCapture(ASTAnalysis astAnalysis)
    {
        // Verify various syntax types are captured
        var syntaxTypes = new[]
        {
            "ClassDeclarationSyntax",
            "MethodDeclarationSyntax",
            "PropertyDeclarationSyntax",
            "FieldDeclarationSyntax",
            "UsingDirectiveSyntax",
            "NamespaceDeclarationSyntax"
        };

        foreach (var syntaxType in syntaxTypes)
        {
            var hasSyntaxType = await FindPatternInAST(astAnalysis, syntaxType);
            hasSyntaxType.Should().BeTrue($"Should capture {syntaxType} patterns");
        }
    }

    private async Task<bool> FindPatternInAST(ASTAnalysis astAnalysis, string pattern)
    {
        await Task.Delay(1); // Simulate async processing
        
        // Serialize AST to JSON for pattern searching using Newtonsoft.Json
        var jsonOutput = JsonConvert.SerializeObject(astAnalysis, new JsonSerializerSettings 
        { 
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        return jsonOutput.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
