using FluentAssertions;
using CSharpAST.Core;
using CSharpAST.Core.Output;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSharpAST.IntegrationTests;

[Collection("CoreFunctionalityTests")]
public class ASTPerformanceIntegrationTests : TestBase
{
    private readonly ILogger<ASTPerformanceIntegrationTests> _performanceLogger;

    public ASTPerformanceIntegrationTests()
    {
        _performanceLogger = _serviceProvider.GetRequiredService<ILogger<ASTPerformanceIntegrationTests>>();
    }

    [Fact]
    public async Task GenerateComplexAST_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TestFiles", "SingleFiles", "CSharp", "LargeComplexFile.cs");
        var stopwatch = Stopwatch.StartNew();

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        stopwatch.Stop();

        // Assert
        astAnalysis.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Complex AST generation should complete within 5 seconds");
        
        _performanceLogger.LogInformation($"Complex AST generation completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GenerateMultipleComplexFiles_ShouldScaleLinearly()
    {
        // Arrange
        var testFilesDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TestFiles", "SingleFiles", "CSharp");
        var testFiles = Directory.GetFiles(testFilesDir, "*.cs").ToList();
        var results = new List<(string fileName, long elapsedMs, int astNodeCount)>();

        // Act
        foreach (var testFile in testFiles)
        {
            var stopwatch = Stopwatch.StartNew();
            var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFile);
            stopwatch.Stop();

            var nodeCount = CountASTNodes(astAnalysis.RootNode);
            results.Add((Path.GetFileName(testFile), stopwatch.ElapsedMilliseconds, nodeCount));
            
            _performanceLogger.LogInformation($"File: {Path.GetFileName(testFile)}, " +
                                 $"Time: {stopwatch.ElapsedMilliseconds}ms, " +
                                 $"Nodes: {nodeCount}");
        }

        // Assert
        results.Should().AllSatisfy(result => 
            result.elapsedMs.Should().BeLessThan(10000, 
                $"Each file should process within 10 seconds, but {result.fileName} took {result.elapsedMs}ms"));

        // Verify performance scales reasonably with complexity
        var averageTimePerNode = results.Average(r => (double)r.elapsedMs / r.astNodeCount);
        averageTimePerNode.Should().BeLessThan(10.0, 
            "Average processing time per AST node should be reasonable");
    }

    [Fact]
    public async Task GenerateASTPerformance_ShouldMaintainPerformance()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TestFiles", "SingleFiles", "CSharp", "ComplexAsyncExample.cs");
        var stopwatch = Stopwatch.StartNew();

        // Act - Generate AST
        var astGenStopwatch = Stopwatch.StartNew();
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        astGenStopwatch.Stop();

        // Act - Create project analysis
        var projectAnalysis = new ProjectAnalysis
        {
            ProjectName = "ComplexAsyncExample",
            Files = new List<ASTAnalysis>
            {
                astAnalysis  // Use the AST analysis directly
            }
        };

        stopwatch.Stop();

        // Assert
        astAnalysis.Should().NotBeNull();
        
        astGenStopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, 
            "AST generation should complete within 3 seconds");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(4000, 
            "Combined operation should complete within 4 seconds");

        _performanceLogger.LogInformation($"AST Generation: {astGenStopwatch.ElapsedMilliseconds}ms, " +
                             $"Total: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GenerateProjectLevelAST_ShouldHandleMultipleFilesEfficiently()
    {
        // Arrange
        var testProjectDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TestFiles", "TestApplications", "BasicDLL");
        var testProjectFile = Path.Combine(testProjectDir, "BasicDLL.csproj");
        var stopwatch = Stopwatch.StartNew();

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
        
        stopwatch.Stop();

        // Assert
        projectAnalysis.Should().NotBeNull();
        projectAnalysis.Files.Should().NotBeEmpty();
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000, 
            "Project-level AST generation should complete within 15 seconds");

        // Verify all files have proper AST tree structure
        foreach (var file in projectAnalysis.Files)
        {
            file.RootNode.Should().NotBeNull();
            // Root node type depends on file type - C# files have CompilationUnitSyntax, Razor files have RazorDocument
            var expectedTypes = new[] { "CompilationUnitSyntax", "RazorDocument", "CompilationUnit" };
            file.RootNode.Type.Should().BeOneOf(expectedTypes, 
                "Root node should be appropriate for the file type");
        }

        _performanceLogger.LogInformation($"Project AST generation for {projectAnalysis.Files.Count} files " +
                             $"completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConcurrentASTGeneration_ShouldNotDegrade()
    {
        // Arrange
        var testFilesDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TestFiles", "SingleFiles", "CSharp");
        var testFiles = Directory.GetFiles(testFilesDir, "*.cs").ToList();
        var concurrencyLevel = Math.Min(Environment.ProcessorCount, testFiles.Count);

        // Act - Sequential processing
        var sequentialStopwatch = Stopwatch.StartNew();
        var sequentialResults = new List<ASTAnalysis>();
        foreach (var file in testFiles)
        {
            var result = await _astGenerator.GenerateFromFileAsync(file);
            sequentialResults.Add(result);
        }
        sequentialStopwatch.Stop();

        // Act - Concurrent processing
        var concurrentStopwatch = Stopwatch.StartNew();
        var concurrentTasks = testFiles.Select(file => _astGenerator.GenerateFromFileAsync(file));
        var concurrentResults = await Task.WhenAll(concurrentTasks);
        concurrentStopwatch.Stop();

        // Assert
        sequentialResults.Should().HaveCount(testFiles.Count);
        concurrentResults.Should().HaveCount(testFiles.Count);
        
        // Concurrent should be faster than sequential for multiple files
        if (testFiles.Count > 1)
        {
            concurrentStopwatch.ElapsedMilliseconds.Should().BeLessThan(
                sequentialStopwatch.ElapsedMilliseconds,
                "Concurrent processing should be faster than sequential");
        }

        _performanceLogger.LogInformation($"Sequential: {sequentialStopwatch.ElapsedMilliseconds}ms, " +
                             $"Concurrent: {concurrentStopwatch.ElapsedMilliseconds}ms, " +
                             $"Speedup: {(double)sequentialStopwatch.ElapsedMilliseconds / concurrentStopwatch.ElapsedMilliseconds:F2}x");
    }

    [Fact]
    public async Task MemoryUsage_ShouldRemainReasonable()
    {
        // Arrange
        var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TestFiles", "SingleFiles", "CSharp", "LargeComplexFile.cs");
        
        // Force garbage collection to get baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(false);

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        var projectAnalysis = new ProjectAnalysis
        {
            ProjectName = "LargeComplexFile",
            Files = new List<ASTAnalysis> { astAnalysis }
        };

        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        astAnalysis.Should().NotBeNull();
        
        // Memory increase should be reasonable (less than 50MB for a single file)
        memoryIncrease.Should().BeLessThan(50 * 1024 * 1024, 
            "Memory usage should remain reasonable");

        _performanceLogger.LogInformation($"Memory increase: {memoryIncrease / 1024.0 / 1024.0:F2} MB");
    }

    private int CountASTNodes(ASTNode node)
    {
        if (node == null) return 0;
        
        int count = 1; // Count current node
        if (node.Children != null)
        {
            count += node.Children.Sum(child => CountASTNodes(child));
        }
        return count;
    }
}
