using FluentAssertions;
using CSharpAST.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpAST.IntegrationTests.CoreFunctionality;

/// <summary>
/// Integration tests for performance and scalability of AST generation
/// </summary>
[Collection("CoreFunctionalityTests")]
public class PerformanceTests : TestBase
{
    private readonly ILogger<PerformanceTests> _logger;

    public PerformanceTests()
    {
        _logger = _serviceProvider.GetRequiredService<ILogger<PerformanceTests>>();
    }

    [Fact]
    public async Task GenerateComplexAST_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "ComplexAsyncExample.cs");
        var maxAllowedTime = TimeSpan.FromSeconds(5); // 5 seconds max
        _logger.LogInformation($"Testing AST generation performance for: {testFilePath}");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        stopwatch.Stop();

        // Assert
        astAnalysis.Should().NotBeNull();
        stopwatch.Elapsed.Should().BeLessThan(maxAllowedTime, 
            $"AST generation should complete within {maxAllowedTime.TotalSeconds} seconds");
        
        _logger.LogInformation($"AST generation completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GenerateProjectLevelAST_ShouldHandleMultipleFilesEfficiently()
    {
        // Arrange
        var testProjectDir = Path.Combine(_testFilesPath, "TestApplications", "BasicDLL");
        var testProjectFile = Path.Combine(testProjectDir, "BasicDLL.csproj");
        var maxAllowedTime = TimeSpan.FromSeconds(10); // 10 seconds max for project
        _logger.LogInformation($"Testing project-level AST generation performance: {testProjectFile}");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var astAnalysis = await _astGenerator.ProcessProjectAsync(testProjectFile);
        stopwatch.Stop();

        // Assert
        astAnalysis.Should().NotBeNull();
        stopwatch.Elapsed.Should().BeLessThan(maxAllowedTime, 
            $"Project AST generation should complete within {maxAllowedTime.TotalSeconds} seconds");
        
        _logger.LogInformation($"Project AST generation completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GenerateMultipleComplexFiles_ShouldScaleLinearly()
    {
        // Arrange
        var testFiles = new[]
        {
            Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncRepository.cs"),
            Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "ComplexAsyncExample.cs"),
            Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "SecurityPatterns.cs")
        }.Where(File.Exists).ToArray();

        if (testFiles.Length == 0)
        {
            _logger.LogWarning("No test files found for scalability test");
            return;
        }

        _logger.LogInformation($"Testing scalability with {testFiles.Length} files");

        // Act & Assert
        var timings = new List<long>();
        
        foreach (var testFile in testFiles)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFile);
            stopwatch.Stop();
            
            astAnalysis.Should().NotBeNull();
            timings.Add(stopwatch.ElapsedMilliseconds);
            
            _logger.LogInformation($"File {Path.GetFileName(testFile)}: {stopwatch.ElapsedMilliseconds}ms");
        }

        // Verify reasonable scaling (no single file should take more than 3x the average)
        var averageTime = timings.Average();
        var maxTime = (double)timings.Max();
        
        maxTime.Should().BeLessOrEqualTo(averageTime * 3, 
            "No single file should take more than 3x the average time");
        
        _logger.LogInformation($"Scalability test completed - Average: {averageTime:F1}ms, Max: {maxTime}ms");
    }

    [Fact]
    public async Task ConcurrentASTGeneration_ShouldNotDegrade()
    {
        // Arrange
        var testFiles = new[]
        {
            Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncRepository.cs"),
            Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "ComplexAsyncExample.cs")
        }.Where(File.Exists).ToArray();

        if (testFiles.Length < 2)
        {
            _logger.LogWarning("Insufficient test files for concurrent test");
            return;
        }

        _logger.LogInformation($"Testing concurrent AST generation with {testFiles.Length} files");

        // Act - Sequential processing
        var sequentialStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var sequentialResults = new List<ASTAnalysis>();
        foreach (var file in testFiles)
        {
            var result = await _astGenerator.GenerateFromFileAsync(file);
            if (result != null)
                sequentialResults.Add(result);
        }
        sequentialStopwatch.Stop();

        // Act - Concurrent processing
        var concurrentStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var concurrentTasks = testFiles.Select(file => _astGenerator.GenerateFromFileAsync(file));
        var concurrentResults = await Task.WhenAll(concurrentTasks);
        concurrentStopwatch.Stop();

        // Assert
        sequentialResults.Should().HaveCount(testFiles.Length);
        concurrentResults.Should().HaveCount(testFiles.Length);
        concurrentResults.Should().OnlyContain(r => r != null);

        // Concurrent should be faster or at least not significantly slower
        var speedupRatio = (double)sequentialStopwatch.ElapsedMilliseconds / concurrentStopwatch.ElapsedMilliseconds;
        speedupRatio.Should().BeGreaterThan(0.7, "Concurrent processing should not be significantly slower");

        _logger.LogInformation($"Sequential: {sequentialStopwatch.ElapsedMilliseconds}ms, " +
                             $"Concurrent: {concurrentStopwatch.ElapsedMilliseconds}ms, " +
                             $"Speedup: {speedupRatio:F2}x");
    }

    [Fact]
    public async Task MemoryUsage_ShouldRemainReasonable()
    {
        // Arrange
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "ComplexAsyncExample.cs");
        _logger.LogInformation($"Testing memory usage for: {testFilePath}");

        // Measure baseline memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(false);

        // Act
        var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
        
        // Measure memory after processing
        var afterProcessingMemory = GC.GetTotalMemory(false);
        var memoryUsed = afterProcessingMemory - initialMemory;

        // Assert
        astAnalysis.Should().NotBeNull();
        memoryUsed.Should().BeLessThan(50 * 1024 * 1024, "Memory usage should be less than 50MB for a single file");
        
        _logger.LogInformation($"Memory used: {memoryUsed / 1024.0 / 1024.0:F2} MB");
    }

    [Fact] 
    public async Task GenerateASTPerformance_ShouldMaintainPerformance()
    {
        // Arrange
        var testFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncRepository.cs");
        var iterations = 5;
        var timings = new List<long>();
        _logger.LogInformation($"Testing performance consistency over {iterations} iterations");

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var astAnalysis = await _astGenerator.GenerateFromFileAsync(testFilePath);
            stopwatch.Stop();
            
            astAnalysis.Should().NotBeNull();
            timings.Add(stopwatch.ElapsedMilliseconds);
            
            _logger.LogInformation($"Iteration {i + 1}: {stopwatch.ElapsedMilliseconds}ms");
        }

        // Assert
        var averageTime = timings.Average();
        var maxTime = timings.Max();
        var minTime = timings.Min();
        
        // Performance should be consistent (max time should not be more than 2x min time)
        (maxTime / (double)minTime).Should().BeLessOrEqualTo(2.0, "Performance should be consistent across iterations");
        averageTime.Should().BeLessThan(1000, "Average time should be under 1 second");
        
        _logger.LogInformation($"Performance stats - Avg: {averageTime:F1}ms, Min: {minTime}ms, Max: {maxTime}ms");
    }
}
