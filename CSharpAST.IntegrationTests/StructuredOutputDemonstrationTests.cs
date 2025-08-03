using FluentAssertions;
using CSharpAST.Core;

namespace CSharpAST.IntegrationTests;

public class StructuredOutputDemonstrationTests : TestBase
{
    [Fact]
    public async Task GenerateStructuredOutput_SingleFile_ShouldCreateOrganizedOutput()
    {
        // Arrange - Test with a single C# file
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncSample.cs");
        var outputDir = CreateStructuredOutputPath("SingleFiles/CSharp/AsyncSample.cs", "GenerateStructuredOutput_SingleFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with structured output
        var generator = ASTGenerator.CreateUnified(verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir, "json");

            // Assert - Verify structured output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected output file exists (ASTGenerator creates the filename)
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.json");
            outputFiles.Should().NotBeEmpty("Should create output file for AsyncSample");
            
            // Verify content
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("AsyncSample.cs", "Output should reference the source file");
            content.Should().Contain("CompilationUnitSyntax", "Output should contain AST structure");
            
            _logger.LogInformation($"Structured output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateStructuredOutput_RazorFile_ShouldCreateOrganizedOutput()
    {
        // Arrange - Test with a Razor file
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "Razor", "RazorSample.cshtml");
        var outputDir = CreateStructuredOutputPath("SingleFiles/Razor/RazorSample.cshtml", "GenerateStructuredOutput_RazorFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with structured output
        var generator = ASTGenerator.CreateUnified(verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir, "json");

            // Assert - Verify structured output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected output file exists (ASTGenerator creates the filename)
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.json");
            outputFiles.Should().NotBeEmpty("Should create output file for RazorSample");
            
            // Verify content
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("RazorSample.cshtml", "Output should reference the source file");
            
            _logger.LogInformation($"Structured Razor output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateStructuredOutput_TestApplication_ShouldCreateOrganizedOutput()
    {
        // Arrange - Test with a test application directory
        var applicationPath = Path.Combine(_testFilesPath, "TestApplications", "BasicDLL");
        var outputDir = CreateApplicationOutputPath("BasicDLL", "GenerateStructuredOutput_TestApplication");
        
        // Assert application directory exists
        Directory.Exists(applicationPath).Should().BeTrue($"Test application should exist: {applicationPath}");

        // Act - Generate AST for all files in the application
        var generator = ASTGenerator.CreateUnified(verbose: true);
        try
        {
            await generator.GenerateASTAsync(applicationPath, outputDir, "json");

            // Assert - Verify structured output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the output directory structure matches TestApplications/BasicDLL/TestName
            var expectedPath = Path.Combine(_outputBasePath, "TestApplications", "BasicDLL", "GenerateStructuredOutput_TestApplication");
            outputDir.Should().Be(expectedPath, "Output path should follow structured pattern");
            
            // Verify output files were created
            var outputFiles = Directory.GetFiles(outputDir, "*.json", SearchOption.AllDirectories);
            outputFiles.Should().NotBeEmpty("Should create output files for the application");
            
            _logger.LogInformation($"Structured application output created at: {outputDir}");
            _logger.LogInformation($"Generated {outputFiles.Length} output files");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public void CreateStructuredOutputPath_ShouldGenerateCorrectPaths()
    {
        // Test the path generation logic
        var csharpPath = CreateStructuredOutputPath("SingleFiles/CSharp/AsyncSample.cs", "TestMethod");
        var expectedCSharpPath = Path.Combine(_outputBasePath, "SingleFiles", "CSharp", "TestMethod");
        csharpPath.Should().Be(expectedCSharpPath);

        var razorPath = CreateStructuredOutputPath("SingleFiles/Razor/RazorSample.cshtml", "TestMethod");
        var expectedRazorPath = Path.Combine(_outputBasePath, "SingleFiles", "Razor", "TestMethod");
        razorPath.Should().Be(expectedRazorPath);

        var appPath = CreateApplicationOutputPath("MVCApp", "TestMethod");
        var expectedAppPath = Path.Combine(_outputBasePath, "TestApplications", "MVCApp", "TestMethod");
        appPath.Should().Be(expectedAppPath);

        var projectPath = CreateProjectOutputPath("TestMethod");
        var expectedProjectPath = Path.Combine(_outputBasePath, "ProjectAnalysis", "TestMethod");
        projectPath.Should().Be(expectedProjectPath);
    }
}
