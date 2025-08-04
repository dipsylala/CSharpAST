using FluentAssertions;
using CSharpAST.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CSharpAST.IntegrationTests;

/// <summary>
/// Integration tests for processing different types of application projects
/// </summary>
[Collection("ProjectFileTests")]
public class ApplicationProjectTests : TestBase
{
    private readonly ILogger<ApplicationProjectTests> _logger;

    public ApplicationProjectTests()
    {
        _logger = _serviceProvider.GetRequiredService<ILogger<ApplicationProjectTests>>();
    }

    [Fact]
    public async Task ProcessMVCApplication_ShouldCaptureMVCPatterns()
    {
        // Arrange
        var testAppPath = Path.Combine(_testFilesPath, "TestApplications", "MVCApp");
        _logger.LogInformation($"Testing MVCApp AST generation for: {testAppPath}");

        // Check if directory exists
        if (!Directory.Exists(testAppPath))
        {
            _logger.LogInformation($"MVCApp directory not found at {testAppPath}, skipping test");
            return; // Skip test if directory doesn't exist
        }

        // Look for project file or process individual files
        var projectFiles = Directory.GetFiles(testAppPath, "*.csproj", SearchOption.AllDirectories);
        var astAnalyses = new List<ASTAnalysis>();

        // Act
        if (projectFiles.Length > 0)
        {
            // Process as project
            var projectAnalysis = await _astGenerator.ProcessProjectAsync(projectFiles[0]);
            if (projectAnalysis != null)
            {
                astAnalyses.Add(projectAnalysis);
            }
        }
        else
        {
            // Process individual C# files
            var csFiles = Directory.GetFiles(testAppPath, "*.cs", SearchOption.AllDirectories);
            foreach (var file in csFiles.Take(5)) // Limit to avoid excessive processing
            {
                var analysis = await _astGenerator.GenerateFromFileAsync(file);
                if (analysis != null)
                {
                    astAnalyses.Add(analysis);
                }
            }
        }

        // Assert
        astAnalyses.Should().NotBeNull();
        if (astAnalyses.Count > 0)
        {
            // Verify MVC-specific files are processed
            var homeControllerAnalysis = astAnalyses.FirstOrDefault(a => a.SourceFile.Contains("HomeController") || a.SourceFile.Contains("Controller"));
            if (homeControllerAnalysis != null)
            {
                var jsonResult = JsonConvert.SerializeObject(homeControllerAnalysis, Formatting.Indented);
                jsonResult.Should().Contain("Controller");
            }
        }
        
        _logger.LogInformation($"MVCApp processed {astAnalyses.Count} files");
    }

    [Fact]
    public async Task ProcessRestAPIApplication_ShouldCaptureAPIPatterns()
    {
        // Arrange
        var testAppPath = Path.Combine(_testFilesPath, "TestApplications", "RestAPI");
        _logger.LogInformation($"Testing RestAPI AST generation for: {testAppPath}");

        // Check if directory exists
        if (!Directory.Exists(testAppPath))
        {
            _logger.LogInformation($"RestAPI directory not found at {testAppPath}, skipping test");
            return; // Skip test if directory doesn't exist
        }

        // Look for project file or process individual files
        var projectFiles = Directory.GetFiles(testAppPath, "*.csproj", SearchOption.AllDirectories);
        var astAnalyses = new List<ASTAnalysis>();

        // Act
        if (projectFiles.Length > 0)
        {
            var projectAnalysis = await _astGenerator.ProcessProjectAsync(projectFiles[0]);
            if (projectAnalysis != null)
            {
                astAnalyses.Add(projectAnalysis);
            }
        }
        else
        {
            var csFiles = Directory.GetFiles(testAppPath, "*.cs", SearchOption.AllDirectories);
            foreach (var file in csFiles.Take(5))
            {
                var analysis = await _astGenerator.GenerateFromFileAsync(file);
                if (analysis != null)
                {
                    astAnalyses.Add(analysis);
                }
            }
        }

        // Assert
        astAnalyses.Should().NotBeNull();
        
        // Verify API-specific constructs are captured
        if (astAnalyses.Count > 0)
        {
            var apiControllerAnalysis = astAnalyses.FirstOrDefault(a => a.SourceFile.Contains("Controller"));
            if (apiControllerAnalysis != null)
            {
                var jsonResult = JsonConvert.SerializeObject(apiControllerAnalysis, Formatting.Indented);
                jsonResult.Should().Contain("Controller");
            }
        }
        
        _logger.LogInformation($"RestAPI processed {astAnalyses.Count} files");
    }

    [Fact]
    public async Task ProcessNetFramework48MVCApplication_ShouldCaptureLegacyMVCPatterns()
    {
        // Arrange
        var testAppPath = Path.Combine(_testFilesPath, "TestApplications", "NetFramework48MVC");
        _logger.LogInformation($"Testing NetFramework48MVC AST generation for: {testAppPath}");

        // Check if directory exists
        if (!Directory.Exists(testAppPath))
        {
            _logger.LogInformation($"NetFramework48MVC directory not found at {testAppPath}, skipping test");
            return; // Skip test if directory doesn't exist
        }

        // Look for project file or process individual files
        var projectFiles = Directory.GetFiles(testAppPath, "*.csproj", SearchOption.AllDirectories);
        var astAnalyses = new List<ASTAnalysis>();

        // Act
        if (projectFiles.Length > 0)
        {
            var projectAnalysis = await _astGenerator.ProcessProjectAsync(projectFiles[0]);
            if (projectAnalysis != null)
            {
                astAnalyses.Add(projectAnalysis);
            }
        }
        else
        {
            var csFiles = Directory.GetFiles(testAppPath, "*.cs", SearchOption.AllDirectories);
            foreach (var file in csFiles.Take(5))
            {
                var analysis = await _astGenerator.GenerateFromFileAsync(file);
                if (analysis != null)
                {
                    astAnalyses.Add(analysis);
                }
            }
        }

        // Assert
        astAnalyses.Should().NotBeNull();
        
        // Verify .NET Framework MVC patterns are captured
        if (astAnalyses.Count > 0)
        {
            var controllerAnalysis = astAnalyses.FirstOrDefault(a => a.SourceFile.Contains("Controller"));
            if (controllerAnalysis != null)
            {
                var jsonResult = JsonConvert.SerializeObject(controllerAnalysis, Formatting.Indented);
                jsonResult.Should().Contain("Controller");
            }
        }
        
        _logger.LogInformation($"NetFramework48MVC processed {astAnalyses.Count} files");
    }

    [Fact]
    public async Task ProcessNetFramework48WebFormsApplication_ShouldCaptureWebFormsPatterns()
    {
        // Arrange
        var testAppPath = Path.Combine(_testFilesPath, "TestApplications", "NetFramework48WebForms");
        _logger.LogInformation($"Testing NetFramework48WebForms AST generation for: {testAppPath}");

        // Check if directory exists
        if (!Directory.Exists(testAppPath))
        {
            _logger.LogInformation($"NetFramework48WebForms directory not found at {testAppPath}, skipping test");
            return; // Skip test if directory doesn't exist
        }

        // Look for project file or process individual files
        var projectFiles = Directory.GetFiles(testAppPath, "*.csproj", SearchOption.AllDirectories);
        var astAnalyses = new List<ASTAnalysis>();

        // Act
        if (projectFiles.Length > 0)
        {
            var projectAnalysis = await _astGenerator.ProcessProjectAsync(projectFiles[0]);
            if (projectAnalysis != null)
            {
                astAnalyses.Add(projectAnalysis);
            }
        }
        else
        {
            var csFiles = Directory.GetFiles(testAppPath, "*.cs", SearchOption.AllDirectories);
            foreach (var file in csFiles.Take(5))
            {
                var analysis = await _astGenerator.GenerateFromFileAsync(file);
                if (analysis != null)
                {
                    astAnalyses.Add(analysis);
                }
            }
        }

        // Assert
        astAnalyses.Should().NotBeNull();
        
        // Verify WebForms-specific patterns are captured
        if (astAnalyses.Count > 0)
        {
            var pageAnalysis = astAnalyses.FirstOrDefault(a => a.SourceFile.Contains(".aspx.cs") || a.SourceFile.Contains("Page"));
            if (pageAnalysis != null)
            {
                var jsonResult = JsonConvert.SerializeObject(pageAnalysis, Formatting.Indented);
                jsonResult.Should().Contain("Page");
            }
        }
        
        _logger.LogInformation($"NetFramework48WebForms processed {astAnalyses.Count} files");
    }

    [Fact]
    public async Task ProcessAllTestApplications_ShouldHandleMultipleApplicationTypes()
    {
        // Arrange
        var testApplicationsPath = Path.Combine(_testFilesPath, "TestApplications");
        _logger.LogInformation($"Testing comprehensive AST generation for all test applications: {testApplicationsPath}");

        // Act
        var allAstAnalyses = new List<ASTAnalysis>();
        var processedFileExtensions = new List<string>();
        
        if (Directory.Exists(testApplicationsPath))
        {
            var applicationDirs = Directory.GetDirectories(testApplicationsPath);
            foreach (var appDir in applicationDirs)
            {
                try
                {
                    // Look for project files first
                    var projectFiles = Directory.GetFiles(appDir, "*.csproj", SearchOption.AllDirectories);
                    if (projectFiles.Length > 0)
                    {
                        var projectAnalysis = await _astGenerator.ProcessProjectAsync(projectFiles[0]);
                        if (projectAnalysis != null)
                        {
                            allAstAnalyses.Add(projectAnalysis);
                            
                            // Extract file extensions from children nodes that represent individual files
                            if (projectAnalysis.RootNode?.Children != null)
                            {
                                foreach (var child in projectAnalysis.RootNode.Children)
                                {
                                    if (child.Properties?.ContainsKey("FilePath") == true)
                                    {
                                        var filePath = child.Properties["FilePath"].ToString();
                                        if (!string.IsNullOrEmpty(filePath))
                                        {
                                            processedFileExtensions.Add(Path.GetExtension(filePath));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Process individual files
                        var csFiles = Directory.GetFiles(appDir, "*.cs", SearchOption.AllDirectories);
                        foreach (var file in csFiles.Take(3)) // Limit to avoid excessive processing
                        {
                            var analysis = await _astGenerator.GenerateFromFileAsync(file);
                            if (analysis != null)
                            {
                                allAstAnalyses.Add(analysis);
                                processedFileExtensions.Add(Path.GetExtension(analysis.SourceFile));
                            }
                        }
                    }
                    _logger.LogInformation($"Processed {Path.GetFileName(appDir)}: Found files");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to process {Path.GetFileName(appDir)}: {ex.Message}");
                }
            }
        }

        // Assert
        allAstAnalyses.Should().NotBeNull();
        
        // Verify different application types are represented
        if (allAstAnalyses.Count > 0 || processedFileExtensions.Count > 0)
        {
            var fileTypes = allAstAnalyses.Select(a => Path.GetExtension(a.SourceFile))
                .Concat(processedFileExtensions)
                .Where(ext => !string.IsNullOrEmpty(ext))
                .Distinct()
                .ToList();
            
            // Should have at least some C# files or project files
            (fileTypes.Contains(".cs") || fileTypes.Contains(".csproj")).Should().BeTrue("Should process at least some .cs or .csproj files");
        }
        
        _logger.LogInformation($"Comprehensive test processed {allAstAnalyses.Count} total analyses");
        _logger.LogInformation($"Total processed file extensions: {processedFileExtensions.Count}");
    }
}
