using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpAST.IntegrationTests.Helpers;

public static class TestServiceProvider
{
    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => 
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Add core services
        services.AddTransient<ASTGenerator>();
        
        return services.BuildServiceProvider();
    }
}

public static class TestFileHelper
{
    public static string GetTestFilePath(string fileName)
    {
        // Test files are copied directly to the output directory
        return Path.Combine(Directory.GetCurrentDirectory(), fileName);
    }

    public static string GetTestFilesDirectory()
    {
        // Test files are in the current output directory
        return Directory.GetCurrentDirectory();
    }

    public static IEnumerable<string> GetAllTestFiles()
    {
        var testDir = GetTestFilesDirectory();
        return Directory.GetFiles(testDir, "*.cs").Where(f => 
            Path.GetFileName(f).StartsWith("Async") || 
            Path.GetFileName(f).StartsWith("Complex") || 
            Path.GetFileName(f).StartsWith("Large") || 
            Path.GetFileName(f).StartsWith("Security"));
    }
}

public static class ASTTestHelpers
{
    public static async Task<bool> ContainsPatternAsync(ASTAnalysis astAnalysis, string pattern)
    {
        await Task.Delay(1); // Simulate async processing
        var jsonOutput = JsonSerializer.Serialize(astAnalysis);
        return jsonOutput.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<bool> ContainsAllPatternsAsync(ASTAnalysis astAnalysis, params string[] patterns)
    {
        var jsonOutput = JsonSerializer.Serialize(astAnalysis);
        return await Task.FromResult(patterns.All(pattern => 
            jsonOutput.Contains(pattern, StringComparison.OrdinalIgnoreCase)));
    }

    public static int CountASTNodes(ASTNode node)
    {
        if (node == null) return 0;
        
        int count = 1; // Count current node
        if (node.Children != null)
        {
            count += node.Children.Sum(child => CountASTNodes(child));
        }
        return count;
    }

    public static FileAnalysis CreateMockFileAnalysis(string fileName, ASTAnalysis astAnalysis)
    {
        return new FileAnalysis
        {
            FilePath = astAnalysis.SourceFile,
            Classes = new List<ClassInfo>(), // TODO: Extract from AST if needed
            Interfaces = new List<InterfaceInfo>(), // TODO: Extract from AST if needed
            Methods = new List<string>(), // TODO: Extract from AST if needed
            Enums = new List<string>(),
            Properties = new List<string>() // TODO: Extract from AST if needed
        };
    }
}

public static class PerformanceTestHelpers
{
    public static async Task<(T result, long elapsedMs)> MeasureAsync<T>(Func<Task<T>> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        return (result, stopwatch.ElapsedMilliseconds);
    }

    public static (T result, long elapsedMs) Measure<T>(Func<T> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = operation();
        stopwatch.Stop();
        return (result, stopwatch.ElapsedMilliseconds);
    }

    public static void LogPerformanceMetrics(ILogger logger, string operation, long elapsedMs, int? itemCount = null)
    {
        var message = $"{operation} completed in {elapsedMs}ms";
        if (itemCount.HasValue)
        {
            message += $" ({itemCount.Value} items, {(double)elapsedMs / itemCount.Value:F2}ms per item)";
        }
        logger.LogInformation(message);
    }
}
