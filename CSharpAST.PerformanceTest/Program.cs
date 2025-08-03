using CSharpAST.Core;
using CSharpAST.Performance;

namespace CSharpAST.PerformanceTest;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== AST Generation Performance Test ===");
        
        // Use current project as test subject
        var testPath = Environment.CurrentDirectory;
        
        // Look for the main CSharpAST.Core project
        var rootPath = FindProjectRoot();
        if (rootPath != null)
        {
            testPath = Path.Combine(rootPath, "CSharpAST.Core");
        }
        
        if (!Directory.Exists(testPath))
        {
            Console.WriteLine($"Test path not found: {testPath}");
            Console.WriteLine("Using current directory...");
            testPath = Environment.CurrentDirectory;
        }

        Console.WriteLine($"Testing AST generation performance on: {testPath}");
        Console.WriteLine();

        try
        {
            // Run performance benchmarks
            var benchmark = new ASTPerformanceBenchmark(verbose: true);
            var summary = await benchmark.RunBenchmarksAsync(testPath, iterations: 3);

            // Display detailed results
            Console.WriteLine("\n=== Detailed Results ===");
            
            if (summary.SingleFileResults.Any())
            {
                Console.WriteLine("\nSingle File Performance:");
                foreach (var result in summary.SingleFileResults)
                {
                    Console.WriteLine($"  {Path.GetFileName(result.FilePath)}:");
                    Console.WriteLine($"    Size: {result.FileSize / 1024.0:F1} KB");
                    Console.WriteLine($"    Standard: {result.StandardAverageMs:F1}ms");
                    Console.WriteLine($"    Optimized: {result.OptimizedAverageMs:F1}ms");
                    Console.WriteLine($"    Speedup: {result.SpeedupFactor:F2}x");
                    Console.WriteLine($"    Time saved: {result.TimesSaved:F1}ms");
                }
            }

            if (summary.MultiFileResult != null)
            {
                Console.WriteLine("\nMultiple Files Performance:");
                var result = summary.MultiFileResult;
                Console.WriteLine($"  Files: {result.FileCount}");
                Console.WriteLine($"  Total size: {result.FileSize / 1024.0:F1} KB");
                Console.WriteLine($"  Standard: {result.StandardAverageMs:F1}ms");
                Console.WriteLine($"  Optimized: {result.OptimizedAverageMs:F1}ms");
                Console.WriteLine($"  Speedup: {result.SpeedupFactor:F2}x");
                Console.WriteLine($"  Time saved: {result.TimesSaved:F1}ms");
            }

            if (summary.ProjectResult != null)
            {
                Console.WriteLine("\nProject Performance:");
                var result = summary.ProjectResult;
                Console.WriteLine($"  Project: {Path.GetFileName(result.FilePath)}");
                Console.WriteLine($"  Standard: {result.StandardAverageMs:F1}ms");
                Console.WriteLine($"  Optimized: {result.OptimizedAverageMs:F1}ms");
                Console.WriteLine($"  Speedup: {result.SpeedupFactor:F2}x");
                Console.WriteLine($"  Time saved: {result.TimesSaved:F1}ms");
            }

            // Test concurrent vs sequential processing
            await TestConcurrencyBenefits(testPath);
            
            // Test thread scaling performance
            await TestThreadScaling(testPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during performance testing: {ex.Message}");
            if (args.Contains("--verbose"))
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static async Task TestConcurrencyBenefits(string testPath)
    {
        Console.WriteLine("\n=== Concurrency Benefits Test ===");
        
        var files = Directory.GetFiles(testPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("bin") && !f.Contains("obj"))
            .Take(10) // Test with first 10 files
            .ToList();

        if (files.Count < 2)
        {
            Console.WriteLine("Not enough files for concurrency testing.");
            return;
        }

        Console.WriteLine($"Testing concurrency with {files.Count} files...");

        // Test different concurrency levels
        var concurrencyLevels = new[] { 1, 2, 4, Environment.ProcessorCount };
        
        foreach (var maxConcurrency in concurrencyLevels)
        {
            using var generator = ASTGenerator.CreateOptimized(maxConcurrency: maxConcurrency);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Process files concurrently
            var tasks = files.Select(file => generator.GenerateFromFileAsync(file)).ToList();
            var results = await Task.WhenAll(tasks);
            
            stopwatch.Stop();
            
            var successCount = results.Count(r => r != null);
            Console.WriteLine($"  Concurrency {maxConcurrency}: {stopwatch.ElapsedMilliseconds}ms ({successCount}/{files.Count} files)");
        }
    }

    static async Task TestThreadScaling(string testPath)
    {
        Console.WriteLine("=== Thread Scaling Analysis ===");
        
        var files = Directory.GetFiles(testPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => new FileInfo(f).Length > 1000) // Only files > 1KB
            .Take(15) // Test with 15 files for better scaling visibility
            .ToList();

        if (files.Count < 5)
        {
            Console.WriteLine("Not enough substantial files for thread scaling testing.");
            return;
        }

        Console.WriteLine($"Testing thread scaling with {files.Count} files...");
        Console.WriteLine("Thread Count | Time (ms) | Speedup | Memory (MB)");
        Console.WriteLine("-------------|-----------|---------|------------");

        // Test various thread counts from 1 to 2x CPU cores
        var threadCounts = new List<int> { 1, 2, 4, 8, 12, 16, Environment.ProcessorCount, Environment.ProcessorCount * 2 }
            .Where(t => t <= 32) // Cap at 32 threads
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        var baselineTime = 0.0;

        foreach (var threadCount in threadCounts)
        {
            var times = new List<long>();
            var memoryUsages = new List<long>();
            
            // Run 3 iterations for each thread count
            for (int iteration = 0; iteration < 3; iteration++)
            {
                // Force garbage collection before measurement
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                var memoryBefore = GC.GetTotalMemory(false);
                
                using var generator = ASTGenerator.CreateOptimized(maxConcurrency: threadCount);
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Process files concurrently
                var tasks = files.Select(file => generator.GenerateFromFileAsync(file)).ToList();
                var results = await Task.WhenAll(tasks);
                
                stopwatch.Stop();
                
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                times.Add(stopwatch.ElapsedMilliseconds);
                memoryUsages.Add(memoryUsed);
            }

            var avgTime = times.Average();
            var avgMemory = memoryUsages.Average() / (1024 * 1024); // Convert to MB
            if (threadCount == 1) baselineTime = avgTime;
            
            var speedup = baselineTime / avgTime;
            
            Console.WriteLine($"{threadCount,12} | {avgTime,9:F1} | {speedup,7:F2}x | {avgMemory,10:F1}");
        }

        Console.WriteLine();
        Console.WriteLine("Notes:");
        Console.WriteLine("- Speedup: Performance improvement compared to single thread");
        Console.WriteLine("- Memory: Peak memory allocation during processing");
        Console.WriteLine($"- CPU Cores: {Environment.ProcessorCount}");
    }

    static string? FindProjectRoot()
    {
        var current = new DirectoryInfo(Environment.CurrentDirectory);
        
        while (current != null)
        {
            // Look for solution file or main project
            if (current.GetFiles("*.sln").Any() || 
                current.GetDirectories("CSharpAST.Core").Any())
            {
                return current.FullName;
            }
            current = current.Parent;
        }
        
        return null;
    }
}
