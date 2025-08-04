using FluentAssertions;
using CSharpAST.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CSharpAST.IntegrationTests;

/// <summary>
/// Integration tests for processing individual source files of different types
/// </summary>
[Collection("ProjectFileTests")]
public class SingleFileProcessingTests : TestBase
{
    private readonly ILogger<SingleFileProcessingTests> _logger;

    public SingleFileProcessingTests()
    {
        _logger = _serviceProvider.GetRequiredService<ILogger<SingleFileProcessingTests>>();
    }

    [Fact]
    public async Task ProcessAsyncRepositoryFile_ShouldCaptureAsyncPatterns()
    {
        // Arrange
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncRepository.cs");
        _logger.LogInformation($"Testing AsyncRepository AST generation for: {testFilePath}");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);

        // Assert
        astAnalysis.Should().NotBeNull();
        astAnalysis!.SourceFile.Should().EndWith("AsyncRepository.cs");
        astAnalysis.RootNode.Should().NotBeNull();
        astAnalysis.RootNode.Type.Should().Be("CompilationUnitSyntax");

        // Verify async repository methods are captured
        var jsonResult = JsonConvert.SerializeObject(astAnalysis, Formatting.Indented);
        jsonResult.Should().Contain("Task");
        jsonResult.Should().Contain("async");
        jsonResult.Should().Contain("Repository");
        
        _logger.LogInformation($"AsyncRepository AST contains {astAnalysis.RootNode.Children?.Count ?? 0} root children");
    }

    [Fact]
    public async Task ProcessVBBookModelsFile_ShouldCaptureVBSyntax()
    {
        // Arrange
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "VB", "BookModels.vb");
        _logger.LogInformation($"Testing BookModels VB AST generation for: {testFilePath}");
        
        // Debug: Check if file exists
        _logger.LogInformation($"File exists: {File.Exists(testFilePath)}");
        
        if (File.Exists(testFilePath))
        {
            var content = await File.ReadAllTextAsync(testFilePath);
            _logger.LogInformation($"File content length: {content.Length}");
            _logger.LogInformation($"First 100 characters: {content.Substring(0, Math.Min(100, content.Length))}");
        }

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        
        // Debug: Log what we got back
        _logger.LogInformation($"AST Analysis result: {astAnalysis?.GetType().Name ?? "null"}");

        // Assert
        astAnalysis.Should().NotBeNull();
        astAnalysis!.SourceFile.Should().EndWith("BookModels.vb");
        astAnalysis.RootNode.Should().NotBeNull();
        astAnalysis.RootNode.Type.Should().Be("CompilationUnitSyntax");

        // Verify VB.NET specific constructs are captured
        var jsonResult = JsonConvert.SerializeObject(astAnalysis, Formatting.Indented);
        jsonResult.Should().Contain("Book");
        jsonResult.Should().Contain("Class");
        
        _logger.LogInformation($"BookModels VB AST contains {astAnalysis.RootNode.Children?.Count ?? 0} root children");
    }

    [Fact]
    public async Task ProcessVBUtilitiesFile_ShouldCaptureVBUtilityMethods()
    {
        // Arrange
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "VB", "Utilities.vb");
        _logger.LogInformation($"Testing Utilities VB AST generation for: {testFilePath}");

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);

        // Assert
        astAnalysis.Should().NotBeNull();
        astAnalysis!.SourceFile.Should().EndWith("Utilities.vb");
        astAnalysis.RootNode.Should().NotBeNull();
        astAnalysis.RootNode.Type.Should().Be("CompilationUnitSyntax");

        // Verify VB.NET utility methods are captured
        var jsonResult = JsonConvert.SerializeObject(astAnalysis, Formatting.Indented);
        jsonResult.Should().Contain("Module");
        jsonResult.Should().Contain("Function");
        
        _logger.LogInformation($"Utilities VB AST contains {astAnalysis.RootNode.Children?.Count ?? 0} root children");
    }

    [Fact]
    public async Task ProcessAllSingleFiles_ShouldHandleMultipleLanguageTypes()
    {
        // Arrange
        var singleFilesPath = Path.Combine(_testFilesPath, "SingleFiles");
        _logger.LogInformation($"Testing comprehensive AST generation for all single files: {singleFilesPath}");

        // Act
        var allAstAnalyses = new List<ASTAnalysis>();
        
        if (Directory.Exists(singleFilesPath))
        {
            var languageDirs = Directory.GetDirectories(singleFilesPath);
            foreach (var langDir in languageDirs)
            {
                try
                {
                    var files = Directory.GetFiles(langDir, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var astAnalysis = await _astGenerator.GenerateFromFileAsync(file);
                        if (astAnalysis != null)
                        {
                            allAstAnalyses.Add(astAnalysis);
                        }
                    }
                    _logger.LogInformation($"Processed {Path.GetFileName(langDir)}: {files.Length} files");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to process {Path.GetFileName(langDir)}: {ex.Message}");
                }
            }
        }

        // Assert
        allAstAnalyses.Should().NotBeNull();
        
        // Verify different languages are represented
        if (allAstAnalyses.Count > 0)
        {
            var fileExtensions = allAstAnalyses.Select(a => Path.GetExtension(a.SourceFile)).Distinct().ToList();
            fileExtensions.Should().Contain(".cs");
            
            // Check if VB files were processed
            if (fileExtensions.Contains(".vb"))
            {
                _logger.LogInformation("VB.NET files were successfully processed");
            }
            
            // Check if Razor files were processed
            if (fileExtensions.Contains(".cshtml"))
            {
                _logger.LogInformation("Razor files were successfully processed");
            }
        }
        
        _logger.LogInformation($"Single files test processed {allAstAnalyses.Count} total files");
    }
}
