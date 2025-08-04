using System.Collections.Concurrent;

namespace CSharpAST.Core.Analysis;

/// <summary>
/// Centralized registry for syntax analyzers that provides efficient access and reuse of analyzer instances.
/// Analyzers are created once at startup and reused throughout the application lifecycle.
/// </summary>
public static class AnalyzerRegistry
{
    private static readonly Lazy<ISyntaxAnalyzer[]> _allAnalyzers = new(() => CreateAnalyzers());
    private static readonly ConcurrentDictionary<string, ISyntaxAnalyzer> _fileAnalyzerCache = new();
    private static readonly ConcurrentDictionary<string, ISyntaxAnalyzer> _projectAnalyzerCache = new();

    /// <summary>
    /// Gets all available syntax analyzers (created once and cached)
    /// </summary>
    public static ISyntaxAnalyzer[] GetAllAnalyzers() => _allAnalyzers.Value;

    /// <summary>
    /// Gets the appropriate analyzer for a specific file type with caching
    /// </summary>
    public static ISyntaxAnalyzer GetAnalyzerForFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return _fileAnalyzerCache.GetOrAdd(extension, ext =>
        {
            var analyzer = _allAnalyzers.Value.FirstOrDefault(a => a.Capabilities.SupportsFile(filePath));
            return analyzer ?? _allAnalyzers.Value[0]; // Default to first analyzer (C#)
        });
    }

    /// <summary>
    /// Gets the appropriate analyzer for a specific project type with caching
    /// </summary>
    public static ISyntaxAnalyzer GetAnalyzerForProject(string projectPath)
    {
        var extension = Path.GetExtension(projectPath).ToLowerInvariant();
        
        return _projectAnalyzerCache.GetOrAdd(extension, ext =>
        {
            var analyzer = _allAnalyzers.Value.FirstOrDefault(a => a.Capabilities.SupportsProject(projectPath));
            return analyzer ?? _allAnalyzers.Value[0]; // Default to first analyzer (C#)
        });
    }

    /// <summary>
    /// Gets analyzers that support a specific file type
    /// </summary>
    public static ISyntaxAnalyzer[] GetAnalyzersForFile(string filePath)
    {
        return _allAnalyzers.Value.Where(a => a.Capabilities.SupportsFile(filePath)).ToArray();
    }

    /// <summary>
    /// Gets analyzers that support a specific project type
    /// </summary>
    public static ISyntaxAnalyzer[] GetAnalyzersForProject(string projectPath)
    {
        return _allAnalyzers.Value.Where(a => a.Capabilities.SupportsProject(projectPath)).ToArray();
    }

    /// <summary>
    /// Checks if any analyzer supports the given file type
    /// </summary>
    public static bool IsFileSupported(string filePath)
    {
        return _allAnalyzers.Value.Any(a => a.Capabilities.SupportsFile(filePath));
    }

    /// <summary>
    /// Checks if any analyzer supports the given project type
    /// </summary>
    public static bool IsProjectSupported(string projectPath)
    {
        return _allAnalyzers.Value.Any(a => a.Capabilities.SupportsProject(projectPath));
    }

    /// <summary>
    /// Gets all supported file extensions across all analyzers
    /// </summary>
    public static string[] GetAllSupportedFileExtensions()
    {
        return _allAnalyzers.Value
            .SelectMany(a => a.Capabilities.SupportedFileExtensions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Gets all supported project extensions across all analyzers
    /// </summary>
    public static string[] GetAllSupportedProjectExtensions()
    {
        return _allAnalyzers.Value
            .SelectMany(a => a.Capabilities.SupportedProjectExtensions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Gets an analyzer by its type name
    /// </summary>
    public static ISyntaxAnalyzer GetAnalyzerByType<T>() where T : ISyntaxAnalyzer
    {
        var analyzer = _allAnalyzers.Value.FirstOrDefault(a => a is T);
        return analyzer ?? throw new InvalidOperationException($"Analyzer of type {typeof(T).Name} not found in registry");
    }

    /// <summary>
    /// Gets an analyzer by its type name (string-based lookup)
    /// </summary>
    public static ISyntaxAnalyzer GetAnalyzerByTypeName(string typeName)
    {
        var analyzer = _allAnalyzers.Value.FirstOrDefault(a => a.GetType().Name == typeName);
        return analyzer ?? throw new InvalidOperationException($"Analyzer of type {typeName} not found in registry");
    }

    /// <summary>
    /// Creates the analyzer instances (called once during lazy initialization)
    /// </summary>
    private static ISyntaxAnalyzer[] CreateAnalyzers()
    {
        return new ISyntaxAnalyzer[]
        {
            new CSharpSyntaxAnalyzer(),
            new VBSyntaxAnalyzer(),
            new RazorSyntaxAnalyzer()
        };
    }

    /// <summary>
    /// Clears the analyzer caches (useful for testing or when analyzer capabilities change)
    /// </summary>
    public static void ClearCaches()
    {
        _fileAnalyzerCache.Clear();
        _projectAnalyzerCache.Clear();
    }
}
