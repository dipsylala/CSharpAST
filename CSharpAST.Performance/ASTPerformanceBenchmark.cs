using System.Diagnostics;
using CSharpAST.Core;
using CSharpAST.Core.Analysis;
using CSharpAST.Core.Processing;
using CSharpAST.Core.Output;

namespace CSharpAST.Performance;

/// <summary>
/// Performance benchmarking tool for c            var analyses = await concurrentGenerator.GenerateFromProjectAsync(projectPath);mparing standard vs. optimized AST generation.
/// </summary>
public class ASTPerformanceBenchmark
{
    private readonly bool _verbose;
    private readonly List<BenchmarkResult> _results = new();

    public ASTPerformanceBenchmark(bool verbose = true)
    {
        _verbose = verbose;
    }

    /// <summary>
    /// Run comprehensive benchmarks comparing standard vs optimized AST generation.
    /// </summary>
    public async Task<BenchmarkSummary> RunBenchmarksAsync(string testPath, int iterations = 3)
    {
        if (_verbose)
            Console.WriteLine($"Starting AST performance benchmark with {iterations} iterations");

        var files = GetTestFiles(testPath);
        if (files.Count == 0)
        {
            throw new ArgumentException($"No C# files found in test path: {testPath}");
        }

        if (_verbose)
            Console.WriteLine($"Found {files.Count} test files");

        // Warm up JIT
        await WarmUpAsync(files.Take(Math.Min(2, files.Count)).ToList());

        // Run single file benchmarks
        var singleFileResults = new List<BenchmarkResult>();
        foreach (var file in files.Take(5)) // Test first 5 files
        {
            var result = await BenchmarkSingleFileAsync(file, iterations);
            singleFileResults.Add(result);
            _results.Add(result);
        }

        // Run multi-file benchmarks
        var multiFileResult = await BenchmarkMultipleFilesAsync(files, iterations);
        _results.Add(multiFileResult);

        // Run project-level benchmarks if applicable
        BenchmarkResult? projectResult = null;
        var projectFiles = Directory.GetFiles(testPath, "*.csproj", SearchOption.AllDirectories);
        if (projectFiles.Length > 0)
        {
            projectResult = await BenchmarkProjectAsync(projectFiles[0], iterations);
            _results.Add(projectResult);
        }

        return CreateSummary(singleFileResults, multiFileResult, projectResult);
    }

    /// <summary>
    /// Benchmark single file processing.
    /// </summary>
    public async Task<BenchmarkResult> BenchmarkSingleFileAsync(string filePath, int iterations = 3)
    {
        if (_verbose)
            Console.WriteLine($"Benchmarking single file: {Path.GetFileName(filePath)}");

        var standardTimes = new List<long>();
        var optimizedTimes = new List<long>();

        // Standard implementation
        var outputManager = new JsonOutputManager(); var standardGenerator = new ASTGenerator(outputManager);
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var analysis = await standardGenerator.GenerateFromFileAsync(filePath);
            sw.Stop();
            standardTimes.Add(sw.ElapsedMilliseconds);
        }

        // Concurrent implementation
        var outputManager2 = new JsonOutputManager(); using var concurrentGenerator = new ASTGenerator(outputManager2, ASTGenerator.ProcessingMode.Concurrent);
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var analysis = await concurrentGenerator.GenerateFromFileAsync(filePath);
            sw.Stop();
            optimizedTimes.Add(sw.ElapsedMilliseconds);
        }

        var fileInfo = new FileInfo(filePath);
        var result = new BenchmarkResult
        {
            TestName = $"SingleFile_{Path.GetFileName(filePath)}",
            FilePath = filePath,
            FileSize = fileInfo.Length,
            StandardAverageMs = standardTimes.Average(),
            OptimizedAverageMs = optimizedTimes.Average(),
            StandardMinMs = standardTimes.Min(),
            StandardMaxMs = standardTimes.Max(),
            OptimizedMinMs = optimizedTimes.Min(),
            OptimizedMaxMs = optimizedTimes.Max(),
            Iterations = iterations
        };

        if (_verbose)
        {
            Console.WriteLine($"  Standard: {result.StandardAverageMs:F1}ms avg ({result.StandardMinMs}-{result.StandardMaxMs}ms)");
            Console.WriteLine($"  Optimized: {result.OptimizedAverageMs:F1}ms avg ({result.OptimizedMinMs}-{result.OptimizedMaxMs}ms)");
            Console.WriteLine($"  Speedup: {result.SpeedupFactor:F2}x");
        }

        return result;
    }

    /// <summary>
    /// Benchmark multiple file processing.
    /// </summary>
    public async Task<BenchmarkResult> BenchmarkMultipleFilesAsync(List<string> filePaths, int iterations = 3)
    {
        if (_verbose)
            Console.WriteLine($"Benchmarking multiple files: {filePaths.Count} files");

        var standardTimes = new List<long>();
        var optimizedTimes = new List<long>();

        // Standard implementation (sequential)
        var outputManager = new JsonOutputManager(); var standardGenerator = new ASTGenerator(outputManager);
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            foreach (var file in filePaths)
            {
                await standardGenerator.GenerateFromFileAsync(file);
            }
            sw.Stop();
            standardTimes.Add(sw.ElapsedMilliseconds);
        }

        // Concurrent implementation
        var outputManager2 = new JsonOutputManager(); using var concurrentGenerator = new ASTGenerator(outputManager2, ASTGenerator.ProcessingMode.Concurrent);
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var tempDir = Path.GetTempPath();
            await concurrentGenerator.GenerateASTAsync(Path.GetDirectoryName(filePaths[0]) ?? "", tempDir);
            sw.Stop();
            optimizedTimes.Add(sw.ElapsedMilliseconds);
        }

        var totalSize = filePaths.Sum(f => new FileInfo(f).Length);
        var result = new BenchmarkResult
        {
            TestName = $"MultipleFiles_{filePaths.Count}files",
            FilePath = Path.GetDirectoryName(filePaths[0]) ?? "",
            FileSize = totalSize,
            FileCount = filePaths.Count,
            StandardAverageMs = standardTimes.Average(),
            OptimizedAverageMs = optimizedTimes.Average(),
            StandardMinMs = standardTimes.Min(),
            StandardMaxMs = standardTimes.Max(),
            OptimizedMinMs = optimizedTimes.Min(),
            OptimizedMaxMs = optimizedTimes.Max(),
            Iterations = iterations
        };

        if (_verbose)
        {
            Console.WriteLine($"  Standard: {result.StandardAverageMs:F1}ms avg ({result.StandardMinMs}-{result.StandardMaxMs}ms)");
            Console.WriteLine($"  Optimized: {result.OptimizedAverageMs:F1}ms avg ({result.OptimizedMinMs}-{result.OptimizedMaxMs}ms)");
            Console.WriteLine($"  Speedup: {result.SpeedupFactor:F2}x");
        }

        return result;
    }

    /// <summary>
    /// Benchmark project processing.
    /// </summary>
    public async Task<BenchmarkResult> BenchmarkProjectAsync(string projectPath, int iterations = 3)
    {
        if (_verbose)
            Console.WriteLine($"Benchmarking project: {Path.GetFileName(projectPath)}");

        var standardTimes = new List<long>();
        var optimizedTimes = new List<long>();

        // Standard implementation
        var outputManager = new JsonOutputManager(); var standardGenerator = new ASTGenerator(outputManager);
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var analysis = await standardGenerator.GenerateFromProjectAsync(projectPath);
            sw.Stop();
            standardTimes.Add(sw.ElapsedMilliseconds);
        }

        // Concurrent implementation
        var outputManager2 = new JsonOutputManager(); using var concurrentGenerator = new ASTGenerator(outputManager2, ASTGenerator.ProcessingMode.Concurrent);
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var analysis = await concurrentGenerator.GenerateFromProjectAsync(projectPath);
            sw.Stop();
            optimizedTimes.Add(sw.ElapsedMilliseconds);
        }

        var result = new BenchmarkResult
        {
            TestName = $"Project_{Path.GetFileNameWithoutExtension(projectPath)}",
            FilePath = projectPath,
            FileSize = 0, // Project size calculation would be complex
            StandardAverageMs = standardTimes.Average(),
            OptimizedAverageMs = optimizedTimes.Average(),
            StandardMinMs = standardTimes.Min(),
            StandardMaxMs = standardTimes.Max(),
            OptimizedMinMs = optimizedTimes.Min(),
            OptimizedMaxMs = optimizedTimes.Max(),
            Iterations = iterations
        };

        if (_verbose)
        {
            Console.WriteLine($"  Standard: {result.StandardAverageMs:F1}ms avg ({result.StandardMinMs}-{result.StandardMaxMs}ms)");
            Console.WriteLine($"  Optimized: {result.OptimizedAverageMs:F1}ms avg ({result.OptimizedMinMs}-{result.OptimizedMaxMs}ms)");
            Console.WriteLine($"  Speedup: {result.SpeedupFactor:F2}x");
        }

        return result;
    }

    private async Task WarmUpAsync(List<string> files)
    {
        if (_verbose)
            Console.WriteLine("Warming up JIT...");

        var outputManager = new JsonOutputManager(); var standardGenerator = new ASTGenerator(outputManager);
        var outputManager2 = new JsonOutputManager(); using var concurrentGenerator = new ASTGenerator(outputManager2, ASTGenerator.ProcessingMode.Concurrent);

        foreach (var file in files)
        {
            try
            {
                await standardGenerator.GenerateFromFileAsync(file);
                await concurrentGenerator.GenerateFromFileAsync(file);
            }
            catch
            {
                // Ignore warmup errors
            }
        }
    }

    private List<string> GetTestFiles(string path)
    {
        if (File.Exists(path) && path.EndsWith(".cs"))
        {
            return new List<string> { path };
        }

        if (Directory.Exists(path))
        {
            return Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("bin") && !f.Contains("obj"))
                .OrderBy(f => new FileInfo(f).Length)
                .ToList();
        }

        return new List<string>();
    }

    private BenchmarkSummary CreateSummary(List<BenchmarkResult> singleFileResults, 
                                         BenchmarkResult multiFileResult, 
                                         BenchmarkResult? projectResult)
    {
        var summary = new BenchmarkSummary
        {
            TotalTests = _results.Count,
            SingleFileResults = singleFileResults,
            MultiFileResult = multiFileResult,
            ProjectResult = projectResult,
            OverallSpeedupFactor = _results.Average(r => r.SpeedupFactor),
            BestSpeedupFactor = _results.Max(r => r.SpeedupFactor),
            WorstSpeedupFactor = _results.Min(r => r.SpeedupFactor),
            TotalStandardTime = _results.Sum(r => r.StandardAverageMs),
            TotalOptimizedTime = _results.Sum(r => r.OptimizedAverageMs)
        };

        if (_verbose)
        {
            Console.WriteLine("\n=== Benchmark Summary ===");
            Console.WriteLine($"Total tests: {summary.TotalTests}");
            Console.WriteLine($"Overall speedup: {summary.OverallSpeedupFactor:F2}x");
            Console.WriteLine($"Best speedup: {summary.BestSpeedupFactor:F2}x");
            Console.WriteLine($"Worst speedup: {summary.WorstSpeedupFactor:F2}x");
            Console.WriteLine($"Total time saved: {summary.TotalStandardTime - summary.TotalOptimizedTime:F1}ms");
        }

        return summary;
    }
}

public class BenchmarkResult
{
    public string TestName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public long FileSize { get; set; }
    public int FileCount { get; set; } = 1;
    public double StandardAverageMs { get; set; }
    public double OptimizedAverageMs { get; set; }
    public long StandardMinMs { get; set; }
    public long StandardMaxMs { get; set; }
    public long OptimizedMinMs { get; set; }
    public long OptimizedMaxMs { get; set; }
    public int Iterations { get; set; }
    public double SpeedupFactor => StandardAverageMs > 0 ? StandardAverageMs / OptimizedAverageMs : 1.0;
    public double TimesSaved => StandardAverageMs - OptimizedAverageMs;
}

public class BenchmarkSummary
{
    public int TotalTests { get; set; }
    public List<BenchmarkResult> SingleFileResults { get; set; } = new();
    public BenchmarkResult? MultiFileResult { get; set; }
    public BenchmarkResult? ProjectResult { get; set; }
    public double OverallSpeedupFactor { get; set; }
    public double BestSpeedupFactor { get; set; }
    public double WorstSpeedupFactor { get; set; }
    public double TotalStandardTime { get; set; }
    public double TotalOptimizedTime { get; set; }
}
