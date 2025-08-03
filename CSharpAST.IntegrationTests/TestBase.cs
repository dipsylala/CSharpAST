using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CSharpAST.Core;
using CSharpAST.TestGeneration;

namespace CSharpAST.IntegrationTests;

public abstract class TestBase
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ASTGenerator _astGenerator;
    protected readonly ITestDataGenerator _testDataGenerator;
    protected readonly ILogger _logger;
    protected readonly string _testFilesPath;
    protected readonly string _outputBasePath;

    protected TestBase()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddTransient<ASTGenerator>();
        services.AddTransient<ITestDataGenerator, TestDataGenerator>();
        
        _serviceProvider = services.BuildServiceProvider();
        _astGenerator = _serviceProvider.GetRequiredService<ASTGenerator>();
        _testDataGenerator = _serviceProvider.GetRequiredService<ITestDataGenerator>();
        _logger = _serviceProvider.GetRequiredService<ILogger<TestBase>>();
        
        // Set up paths relative to the solution root
        var solutionRoot = GetSolutionRoot();
        _testFilesPath = Path.Combine(solutionRoot, "TestFiles");
        _outputBasePath = Path.Combine(solutionRoot, "Output");
        
        // Ensure output directory exists
        Directory.CreateDirectory(_outputBasePath);
    }

    /// <summary>
    /// Creates a structured output path that mirrors the TestFiles structure
    /// </summary>
    /// <param name="relativeTestPath">Relative path from TestFiles (e.g., "SingleFiles/CSharp/AsyncSample.cs")</param>
    /// <param name="testName">Name of the test being run</param>
    /// <returns>Full output directory path maintaining the directory structure</returns>
    protected string CreateStructuredOutputPath(string relativeTestPath, string testName)
    {
        // Remove TestFiles from the path if it exists
        var cleanPath = relativeTestPath.Replace("TestFiles/", "").Replace("TestFiles\\", "");
        
        // Get the directory structure from the test file path
        var directory = Path.GetDirectoryName(cleanPath) ?? "";
        
        // Create output directory structure: Output/{directory}/{testName}/
        var outputDir = Path.Combine(_outputBasePath, directory, testName);
        Directory.CreateDirectory(outputDir);
        
        // Return the directory path (ASTGenerator will create the filename)
        return outputDir;
    }

    /// <summary>
    /// Creates output path for test applications
    /// </summary>
    /// <param name="applicationName">Name of the test application (e.g., "BasicDLL", "RestAPI", "MVCApp")</param>
    /// <param name="testName">Name of the test being run</param>
    /// <returns>Full output directory path for the application</returns>
    protected string CreateApplicationOutputPath(string applicationName, string testName)
    {
        var outputDir = Path.Combine(_outputBasePath, "TestApplications", applicationName, testName);
        Directory.CreateDirectory(outputDir);
        return outputDir;
    }

    /// <summary>
    /// Creates output path for project-wide analysis
    /// </summary>
    /// <param name="testName">Name of the test being run</param>
    /// <returns>Full output directory path for project analysis</returns>
    protected string CreateProjectOutputPath(string testName)
    {
        var outputDir = Path.Combine(_outputBasePath, "ProjectAnalysis", testName);
        Directory.CreateDirectory(outputDir);
        return outputDir;
    }

    /// <summary>
    /// Cleans up output directory for a specific test
    /// </summary>
    /// <param name="outputPath">Path to clean up</param>
    protected void CleanupOutput(string outputPath)
    {
        try
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup output directory: {OutputPath}", outputPath);
        }
    }

    private static string GetSolutionRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        
        // Walk up the directory tree to find the solution root
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        
        if (directory == null)
        {
            throw new InvalidOperationException("Could not find solution root directory");
        }
        
        return directory.FullName;
    }
}
