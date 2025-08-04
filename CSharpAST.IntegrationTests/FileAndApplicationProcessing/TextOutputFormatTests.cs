using FluentAssertions;
using CSharpAST.Core;
using CSharpAST.Core.OutputManager;

namespace CSharpAST.IntegrationTests;

/// <summary>
/// Integration tests for Text output format generation
/// Tests Text output format with proper readable text structure validation
/// </summary>
[Collection("FileAndApplicationTests")]
public class TextOutputFormatTests : TestBase
{
    [Fact]
    public async Task GenerateTextOutput_SingleCSharpFile_ShouldCreateReadableTextStructure()
    {
        // Arrange - Test with a single C# file using Text output
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncSample.cs");
        var outputDir = CreateStructuredOutputPath("SingleFiles/CSharp/AsyncSample.cs", "GenerateTextOutput_SingleCSharpFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with Text output
        var outputManager = new TextOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

            // Assert - Verify Text output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected Text output file exists
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.txt");
            outputFiles.Should().NotBeEmpty("Should create Text output file for AsyncSample");
            
            // Verify Text content structure
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("AsyncSample.cs", "Text output should reference the source file");
            content.Should().Contain("CompilationUnitSyntax", "Text output should contain AST structure");
            content.Should().Contain("Syntax Tree:", "Text should contain Syntax Tree section");
            content.Should().NotContain("\"", "Text output should not contain JSON quotes");
            content.Should().NotContain("{", "Text output should not contain JSON braces");
            
            _logger.LogInformation($"Text output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateTextOutput_RazorFile_ShouldCreateReadableTextStructure()
    {
        // Arrange - Test with a Razor file using Text output
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "Razor", "RazorSample.cshtml");
        var outputDir = CreateStructuredOutputPath("SingleFiles/Razor/RazorSample.cshtml", "GenerateTextOutput_RazorFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with Text output
        var outputManager = new TextOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

            // Assert - Verify Text output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected Text output file exists
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.txt");
            outputFiles.Should().NotBeEmpty("Should create Text output file for RazorSample");
            
            // Verify Text content
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("RazorSample.cshtml", "Text output should reference the source file");
            content.Should().Contain("Syntax Tree:", "Text should contain Syntax Tree section");
            content.Should().NotContain("\"Type\":", "Text output should not contain JSON-formatted properties");
            content.Should().NotContain("\"SourceFile\":", "Text output should not contain JSON structure markers");
            
            _logger.LogInformation($"Text Razor output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateTextOutput_VBNetFile_ShouldCreateReadableTextStructure()
    {
        // Arrange - Test with a VB.NET file using Text output
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "VB", "BookService.vb");
        var outputDir = CreateStructuredOutputPath("SingleFiles/VB/BookService.vb", "GenerateTextOutput_VBNetFile");
        
        // Assert test file exists
        File.Exists(testFilePath).Should().BeTrue($"Test file should exist: {testFilePath}");

        // Act - Generate AST with Text output
        var outputManager = new TextOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            await generator.GenerateASTAsync(testFilePath, outputDir);

            // Assert - Verify Text output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the expected Text output file exists
            var expectedFileName = Path.GetFileNameWithoutExtension(testFilePath);
            var outputFiles = Directory.GetFiles(outputDir, $"{expectedFileName}*.txt");
            outputFiles.Should().NotBeEmpty("Should create Text output file for BookService");
            
            // Verify Text content
            var outputFile = outputFiles.First();
            var content = await File.ReadAllTextAsync(outputFile);
            content.Should().Contain("BookService.vb", "Text output should reference the source file");
            content.Should().Contain("BookService", "Text output should contain VB.NET class information");
            content.Should().Contain("Syntax Tree:", "Text should contain Syntax Tree section");
            content.Should().NotContain("\"", "Text output should not contain JSON quotes");
            content.Should().NotContain("{", "Text output should not contain JSON braces");
            
            _logger.LogInformation($"Text VB.NET output created at: {outputFile}");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateTextOutput_ProjectApplication_ShouldCreateReadableTextStructure()
    {
        // Arrange - Test with a project application using Text output
        var applicationPath = Path.Combine(_testFilesPath, "TestApplications", "BasicDLL", "BasicDLL.csproj");
        var outputDir = CreateApplicationOutputPath("BasicDLL", "GenerateTextOutput_ProjectApplication");
        
        // Assert application project file exists
        File.Exists(applicationPath).Should().BeTrue($"Test application project should exist: {applicationPath}");

        // Act - Generate AST for the application project with Text output
        var outputManager = new TextOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            await generator.GenerateASTAsync(applicationPath, outputDir);

            // Assert - Verify Text output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify the output directory structure matches TestApplications/BasicDLL/TestName
            var expectedPath = Path.Combine(_outputBasePath, "TestApplications", "BasicDLL", "GenerateTextOutput_ProjectApplication");
            outputDir.Should().Be(expectedPath, "Output path should follow structured pattern");
            
            // Verify Text output files were created
            var outputFiles = Directory.GetFiles(outputDir, "*.txt", SearchOption.AllDirectories);
            outputFiles.Should().NotBeEmpty("Should create Text output files for the application");
            
            // Verify at least one file contains readable text format
            var validTextFound = false;
            foreach (var outputFile in outputFiles)
            {
                var content = await File.ReadAllTextAsync(outputFile);
                if (content.Contains("Syntax Tree:") && !content.Contains("{") && !content.Contains("\""))
                {
                    validTextFound = true;
                    content.Should().Contain("Syntax Tree:", "Text should contain Syntax Tree section");
                    content.Should().NotContain("\"", "Text output should not contain JSON quotes");
                    break;
                }
            }
            validTextFound.Should().BeTrue("At least one file should contain valid text format");
            
            _logger.LogInformation($"Text application output created at: {outputDir}");
            _logger.LogInformation($"Generated {outputFiles.Length} Text output files");
        }
        finally
        {
            generator.Dispose();
        }
    }

    [Fact]
    public async Task GenerateTextOutput_NetFrameworkApplication_ShouldCreateReadableTextStructure()
    {
        // Arrange - Test with .NET Framework 4.8 console application source files using Text output
        var sourceFiles = new[]
        {
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Program.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "BusinessLogic.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Models", "Product.cs"),
            Path.Combine(_testFilesPath, "TestApplications", "NetFramework48Console", "Services", "ProductService.cs")
        };
        
        var outputDir = CreateApplicationOutputPath("NetFramework48Console", "GenerateTextOutput_NetFrameworkApplication");
        
        // Assert all source files exist
        foreach (var sourceFile in sourceFiles)
        {
            File.Exists(sourceFile).Should().BeTrue($"Source file should exist: {sourceFile}");
        }

        // Act - Generate AST for each source file with Text output
        var outputManager = new TextOutputManager();
        var generator = new ASTGenerator(outputManager, verbose: true);
        try
        {
            foreach (var sourceFile in sourceFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                var fileOutputDir = Path.Combine(outputDir, fileName);
                await generator.GenerateASTAsync(sourceFile, fileOutputDir);
            }

            // Assert - Verify Text output was created
            Directory.Exists(outputDir).Should().BeTrue("Output directory should be created");
            
            // Verify Text output files were created for each source file
            var outputFiles = Directory.GetFiles(outputDir, "*.txt", SearchOption.AllDirectories);
            outputFiles.Should().NotBeEmpty("Should create Text output files for the .NET Framework application");
            
            // Verify specific Text content and format
            var outputContent = string.Empty;
            var validTextCount = 0;
            foreach (var outputFile in outputFiles)
            {
                var content = await File.ReadAllTextAsync(outputFile);
                outputContent += content;
                
                if (content.Contains("Syntax Tree:") && !content.Contains("{") && !content.Contains("\""))
                {
                    validTextCount++;
                    content.Should().Contain("Syntax Tree:", "Text should contain Syntax Tree section");
                    content.Should().NotContain("\"", "Text output should not contain JSON quotes");
                }
            }
            
            validTextCount.Should().BeGreaterThan(0, "At least one file should contain valid text format");
            outputContent.Should().Contain("NetFramework48Console", "Text output should reference the namespace");
            outputContent.Should().Contain("Product", "Text output should contain the Product class");
            outputContent.Should().Contain("ProductService", "Text output should contain the ProductService class");
            
            _logger.LogInformation($"Text .NET Framework output created at: {outputDir}");
            _logger.LogInformation($"Generated {outputFiles.Length} Text output files");
        }
        finally
        {
            generator.Dispose();
        }
    }
}
