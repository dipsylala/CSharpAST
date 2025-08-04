using FluentAssertions;
using CSharpAST.Core;
using CSharpAST.Core.Output;

namespace CSharpAST.IntegrationTests;

public class StructuredOutputDemonstrationTests : TestBase
{
    private static IOutputManager CreateOutputManager(string format = "json")
    {
        return format.ToLowerInvariant() switch
        {
            "json" => new JsonOutputManager(),
            "text" or "txt" => new TextOutputManager(),
            _ => new JsonOutputManager() // Default fallback
        };
    }
    [Fact]
    public async Task GenerateStructuredOutput_SingleFile_ShouldCreateOrganizedOutput()
    {
        // Arrange - Test with a single C# file
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncSample.cs");
        var outputDir = CreateStructuredOutputPath("SingleFiles/CSharp/AsyncSample.cs", "GenerateStructuredOutput_SingleFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with structured output
        var outputManager = new JsonOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true); // Use concurrent mode (default)
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

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
        var generator = new ASTGenerator(CreateOutputManager(), verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

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
        // Arrange - Test with a test application project file
        var applicationPath = Path.Combine(_testFilesPath, "TestApplications", "BasicDLL", "BasicDLL.csproj");
        var outputDir = CreateApplicationOutputPath("BasicDLL", "GenerateStructuredOutput_TestApplication");
        
        // Assert application project file exists
        File.Exists(applicationPath).Should().BeTrue($"Test application project should exist: {applicationPath}");

        // Act - Generate AST for the application project
        var generator = new ASTGenerator(CreateOutputManager(), verbose: true);
        try
        {
            await generator.GenerateASTAsync(applicationPath, outputDir);

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

    [Fact]
    public async Task GenerateStructuredOutput_VBFile_ShouldCreateOrganizedOutput()
    {
        // Arrange - Test with a VB.NET file
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "VB", "BookService.vb");
        var outputDir = CreateStructuredOutputPath("SingleFiles/VB/BookService.vb", "GenerateStructuredOutput_VBFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with structured output
        var generator = new ASTGenerator(CreateOutputManager(), verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

            // Assert - Verify structured output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected output file exists (ASTGenerator creates the filename)
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.json");
            outputFiles.Should().NotBeEmpty("Should create output file for BookService");
            
            // Verify content
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("BookService.vb", "Output should reference the source file");
            content.Should().Contain("BookService", "Output should contain VB.NET class information");
            
            _logger.LogInformation($"Structured VB.NET output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateStructuredOutput_VBApplication_ShouldCreateOrganizedOutput()
    {
        // Arrange - Test with a VB application project file
        var applicationPath = Path.Combine(_testFilesPath, "TestApplications", "VBConsoleApp", "VBConsoleApp.vbproj");
        var outputDir = CreateApplicationOutputPath("VBConsoleApp", "GenerateStructuredOutput_VBApplication");
        
        // Assert application project file exists
        File.Exists(applicationPath).Should().BeTrue($"Test VB application project should exist: {applicationPath}");

        // Act - Generate AST for the VB application project
        var generator = new ASTGenerator(CreateOutputManager(), verbose: true);
        try
        {
            await generator.GenerateASTAsync(applicationPath, outputDir);

            // Assert - Verify structured output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the output directory structure matches TestApplications/VBConsoleApp/TestName
            var expectedPath = Path.Combine(_outputBasePath, "TestApplications", "VBConsoleApp", "GenerateStructuredOutput_VBApplication");
            outputDir.Should().Be(expectedPath, "Output path should follow structured pattern");
            
            // Verify output files were created
            var outputFiles = Directory.GetFiles(outputDir, "*.json", SearchOption.AllDirectories);
            outputFiles.Should().NotBeEmpty("Should create output files for the VB application");
            
            _logger.LogInformation($"Structured VB application output created at: {outputDir}");
            _logger.LogInformation($"Generated {outputFiles.Length} output files");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateStructuredOutput_NetFramework48Console_ShouldCreateOrganizedOutput()
    {
        // Arrange - Test with .NET Framework 4.8 console application source files
        var sourceFiles = new[]
        {
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Program.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "BusinessLogic.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Models", "Product.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Services", "ProductService.cs")
        };
        
        var outputDir = CreateApplicationOutputPath("NetFramework48Console", "GenerateStructuredOutput_NetFramework48Console");
        
        // Assert all source files exist
        foreach (var sourceFile in sourceFiles)
        {
            File.Exists(sourceFile).Should().BeTrue($"Source file should exist: {sourceFile}");
        }

        // Act - Generate AST for each source file
        var generator = new ASTGenerator(CreateOutputManager(), verbose: true);
        try
        {
            foreach (var sourceFile in sourceFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                var fileOutputDir = Path.Combine(outputDir, fileName);
                await generator.GenerateASTAsync(sourceFile, fileOutputDir);
            }

            // Assert - Verify structured output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify output files were created for each source file
            var outputFiles = Directory.GetFiles(outputDir, "*.json", SearchOption.AllDirectories);
            outputFiles.Should().NotBeEmpty("Should create output files for the .NET Framework application");
            
            // Verify specific files were processed
            var outputContent = string.Empty;
            foreach (var outputFile in outputFiles)
            {
                var content = await File.ReadAllTextAsync(outputFile);
                outputContent += content;
            }
            
            outputContent.Should().Contain("NetFramework48Console", "Output should reference the namespace");
            outputContent.Should().Contain("Product", "Output should contain the Product class");
            outputContent.Should().Contain("ProductService", "Output should contain the ProductService class");
            
            _logger.LogInformation($"Structured .NET Framework output created at: {outputDir}");
            _logger.LogInformation($"Generated {outputFiles.Length} output files");
        }
        finally
        {
            generator.Dispose();
        }
    }
}
