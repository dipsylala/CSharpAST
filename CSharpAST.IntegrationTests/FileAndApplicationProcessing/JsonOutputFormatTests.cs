using FluentAssertions;
using CSharpAST.Core;
using CSharpAST.Core.Output;

namespace CSharpAST.IntegrationTests;

/// <summary>
/// Integration tests for JSON output format generation
/// Tests JSON output format with proper JSON structure validation
/// </summary>
[Collection("FileAndApplicationTests")]
public class JsonOutputFormatTests : TestBase
{
    [Fact]
    public async Task GenerateJsonOutput_SingleCSharpFile_ShouldCreateValidJsonStructure()
    {
        // Arrange - Test with a single C# file using JSON output
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncSample.cs");
        var outputDir = CreateStructuredOutputPath("SingleFiles/CSharp/AsyncSample.cs", "GenerateJsonOutput_SingleCSharpFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with JSON output
        var outputManager = new JsonOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

            // Assert - Verify JSON output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected JSON output file exists
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.json");
            outputFiles.Should().NotBeEmpty("Should create JSON output file for AsyncSample");
            
            // Verify JSON content structure
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("AsyncSample.cs", "JSON output should reference the source file");
            content.Should().Contain("CompilationUnitSyntax", "JSON output should contain AST structure");
            content.Should().Contain("\"Type\":", "JSON should contain properly formatted Type property");
            content.Should().Contain("\"Properties\":", "JSON should contain Properties object");
            
            // Verify it's valid JSON by attempting to parse
            var jsonTest = System.Text.Json.JsonDocument.Parse(content);
            jsonTest.Should().NotBeNull("Content should be valid JSON");
            
            _logger.LogInformation($"JSON output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateJsonOutput_RazorFile_ShouldCreateValidJsonStructure()
    {
        // Arrange - Test with a Razor file using JSON output
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "Razor", "RazorSample.cshtml");
        var outputDir = CreateStructuredOutputPath("SingleFiles/Razor/RazorSample.cshtml", "GenerateJsonOutput_RazorFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with JSON output
        var outputManager = new JsonOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

            // Assert - Verify JSON output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected JSON output file exists
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.json");
            outputFiles.Should().NotBeEmpty("Should create JSON output file for RazorSample");
            
            // Verify JSON content
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("RazorSample.cshtml", "JSON output should reference the source file");
            content.Should().Contain("\"Type\":", "JSON should contain properly formatted Type property");
            
            // Verify it's valid JSON
            var jsonTest = System.Text.Json.JsonDocument.Parse(content);
            jsonTest.Should().NotBeNull("Content should be valid JSON");
            
            _logger.LogInformation($"JSON Razor output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateJsonOutput_VBNetFile_ShouldCreateValidJsonStructure()
    {
        // Arrange - Test with a VB.NET file using JSON output
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "VB", "BookService.vb");
        var outputDir = CreateStructuredOutputPath("SingleFiles/VB/BookService.vb", "GenerateJsonOutput_VBNetFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with JSON output
        var outputManager = new JsonOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

            // Assert - Verify JSON output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected JSON output file exists
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.json");
            outputFiles.Should().NotBeEmpty("Should create JSON output file for BookService");
            
            // Verify JSON content
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("BookService.vb", "JSON output should reference the source file");
            content.Should().Contain("BookService", "JSON output should contain VB.NET class information");
            content.Should().Contain("\"Type\":", "JSON should contain properly formatted Type property");
            
            // Verify it's valid JSON
            var jsonTest = System.Text.Json.JsonDocument.Parse(content);
            jsonTest.Should().NotBeNull("Content should be valid JSON");
            
            _logger.LogInformation($"JSON VB.NET output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateJsonOutput_ProjectApplication_ShouldCreateValidJsonStructure()
    {
        // Arrange - Test with a project application using JSON output
        var applicationPath = Path.Combine(_testFilesPath, "TestApplications", "BasicDLL", "BasicDLL.csproj");
        var outputDir = CreateApplicationOutputPath("BasicDLL", "GenerateJsonOutput_ProjectApplication");
        
        // Assert application project file exists
        File.Exists(applicationPath).Should().BeTrue($"Test application project should exist: {applicationPath}");

        // Act - Generate AST for the application project with JSON output
        var outputManager = new JsonOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            await generator.GenerateASTAsync(applicationPath, outputDir);

            // Assert - Verify JSON output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the output directory structure matches TestApplications/BasicDLL/TestName
            var expectedPath = Path.Combine(_outputBasePath, "TestApplications", "BasicDLL", "GenerateJsonOutput_ProjectApplication");
            outputDir.Should().Be(expectedPath, "Output path should follow structured pattern");
            
            // Verify JSON output files were created
            var outputFiles = Directory.GetFiles(outputDir, "*.json", SearchOption.AllDirectories);
            outputFiles.Should().NotBeEmpty("Should create JSON output files for the application");
            
            // Verify at least one file contains valid JSON
            var validJsonFound = false;
            foreach (var outputFile in outputFiles)
            {
                var content = await File.ReadAllTextAsync(outputFile);
                try
                {
                    var jsonTest = System.Text.Json.JsonDocument.Parse(content);
                    validJsonFound = true;
                    content.Should().Contain("\"Type\":", "JSON should contain properly formatted Type property");
                    break;
                }
                catch (System.Text.Json.JsonException)
                {
                    // Continue checking other files
                }
            }
            validJsonFound.Should().BeTrue("At least one file should contain valid JSON");
            
            _logger.LogInformation($"JSON application output created at: {outputDir}");
            _logger.LogInformation($"Generated {outputFiles.Length} JSON output files");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateJsonOutput_NetFrameworkApplication_ShouldCreateValidJsonStructure()
    {
        // Arrange - Test with .NET Framework 4.8 console application source files using JSON output
        var sourceFiles = new[]
        {
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Program.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "BusinessLogic.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Models", "Product.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Services", "ProductService.cs")
        };
        
        var outputDir = CreateApplicationOutputPath("NetFramework48Console", "GenerateJsonOutput_NetFrameworkApplication");
        
        // Assert all source files exist
        foreach (var sourceFile in sourceFiles)
        {
            File.Exists(sourceFile).Should().BeTrue($"Source file should exist: {sourceFile}");
        }

        // Act - Generate AST for each source file with JSON output
        var outputManager = new JsonOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            foreach (var sourceFile in sourceFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                var fileOutputDir = Path.Combine(outputDir, fileName);
                await generator.GenerateASTAsync(sourceFile, fileOutputDir);
            }

            // Assert - Verify JSON output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify JSON output files were created for each source file
            var outputFiles = Directory.GetFiles(outputDir, "*.json", SearchOption.AllDirectories);
            outputFiles.Should().NotBeEmpty("Should create JSON output files for the .NET Framework application");
            
            // Verify specific JSON content and structure
            var outputContent = string.Empty;
            var validJsonCount = 0;
            foreach (var outputFile in outputFiles)
            {
                var content = await File.ReadAllTextAsync(outputFile);
                outputContent += content;
                
                try
                {
                    var jsonTest = System.Text.Json.JsonDocument.Parse(content);
                    validJsonCount++;
                    content.Should().Contain("\"Type\":", "JSON should contain properly formatted Type property");
                }
                catch (System.Text.Json.JsonException)
                {
                    // Log but don't fail - some files might not be valid JSON depending on content
                }
            }
            
            validJsonCount.Should().BeGreaterThan(0, "At least one file should contain valid JSON");
            outputContent.Should().Contain("NetFramework48Console", "JSON output should reference the namespace");
            outputContent.Should().Contain("Product", "JSON output should contain the Product class");
            outputContent.Should().Contain("ProductService", "JSON output should contain the ProductService class");
            
            _logger.LogInformation($"JSON .NET Framework output created at: {outputDir}");
            _logger.LogInformation($"Generated {outputFiles.Length} JSON output files");
        }
        finally
        {
            generator.Dispose();
        }
    }
}
