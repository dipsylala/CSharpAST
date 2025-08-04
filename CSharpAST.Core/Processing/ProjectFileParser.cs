using System.Xml.Linq;
using CSharpAST.Core.Analysis;

namespace CSharpAST.Core.Processing;

/// <summary>
/// Parses MSBuild project files (.csproj, .vbproj) to extract included source files.
/// Handles both legacy and SDK-style project formats.
/// </summary>
public static class ProjectFileParser
{
    /// <summary>
    /// Extracts the source files that are included in the project.
    /// For SDK-style projects, this includes implicit file inclusions.
    /// For legacy projects, this reads explicit ItemGroup entries.
    /// </summary>
    public static List<string> GetIncludedSourceFiles(string projectPath, IEnumerable<ISyntaxAnalyzer> analyzers)
    {
        if (!File.Exists(projectPath))
            return new List<string>();

        var projectDir = Path.GetDirectoryName(projectPath);
        if (string.IsNullOrEmpty(projectDir))
            return new List<string>();

        try
        {
            var projectXml = XDocument.Load(projectPath);
            var isSDKProject = IsSDKStyleProject(projectXml);

            if (isSDKProject)
            {
                return GetSDKStyleIncludedFiles(projectXml, projectDir, analyzers);
            }
            else
            {
                return GetLegacyStyleIncludedFiles(projectXml, projectDir, analyzers);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to parse project file {projectPath}: {ex.Message}");
            // Fallback to directory scanning
            return GetFallbackIncludedFiles(projectDir, analyzers);
        }
    }

    /// <summary>
    /// Determines if this is an SDK-style project (modern format).
    /// </summary>
    private static bool IsSDKStyleProject(XDocument projectXml)
    {
        var projectElement = projectXml.Root;
        return projectElement?.Attribute("Sdk") != null;
    }

    /// <summary>
    /// Gets included files for SDK-style projects.
    /// These projects include files implicitly based on common patterns.
    /// </summary>
    private static List<string> GetSDKStyleIncludedFiles(XDocument projectXml, string projectDir, IEnumerable<ISyntaxAnalyzer> analyzers)
    {
        var includedFiles = new List<string>();

        // Resolve to absolute path to handle relative paths correctly in different execution contexts
        var absoluteProjectDir = Path.GetFullPath(projectDir);
        
        if (!Directory.Exists(absoluteProjectDir))
        {
            return includedFiles;
        }
        
        // For SDK-style projects, files are included implicitly
        // Get all files in the project directory that match supported patterns
        var allFiles = Directory.GetFiles(absoluteProjectDir, "*", SearchOption.AllDirectories)
            .Where(f => !IsExcludedPath(f))
            .ToList();

        // Filter to only files supported by analyzers
        foreach (var file in allFiles)
        {
            if (analyzers.Any(analyzer => analyzer.Capabilities.SupportsFile(file)))
            {
                includedFiles.Add(file);
            }
        }

        // Remove explicitly excluded files
        var excludedFiles = GetExplicitlyExcludedFiles(projectXml, absoluteProjectDir);
        includedFiles = includedFiles.Except(excludedFiles, StringComparer.OrdinalIgnoreCase).ToList();

        // Add explicitly included files
        var explicitlyIncluded = GetExplicitlyIncludedFiles(projectXml, absoluteProjectDir, analyzers);
        includedFiles.AddRange(explicitlyIncluded);

        return includedFiles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Gets included files for legacy-style projects.
    /// These projects explicitly list all included files.
    /// </summary>
    private static List<string> GetLegacyStyleIncludedFiles(XDocument projectXml, string projectDir, IEnumerable<ISyntaxAnalyzer> analyzers)
    {
        var includedFiles = new List<string>();

        // Legacy projects explicitly list included files
        var itemGroups = projectXml.Descendants("ItemGroup");

        foreach (var itemGroup in itemGroups)
        {
            // Look for Compile, Content, None, and other item types that might contain source files
            var items = itemGroup.Elements().Where(e => 
                e.Name.LocalName == "Compile" || 
                e.Name.LocalName == "Content" ||
                e.Name.LocalName == "None" ||
                e.Name.LocalName == "EmbeddedResource");

            foreach (var item in items)
            {
                var include = item.Attribute("Include")?.Value;
                if (!string.IsNullOrEmpty(include))
                {
                    var fullPath = Path.Combine(projectDir, include);
                    if (File.Exists(fullPath) && analyzers.Any(analyzer => analyzer.Capabilities.SupportsFile(fullPath)))
                    {
                        includedFiles.Add(fullPath);
                    }
                }
            }
        }

        return includedFiles;
    }

    /// <summary>
    /// Gets files that are explicitly excluded in the project file.
    /// </summary>
    private static List<string> GetExplicitlyExcludedFiles(XDocument projectXml, string projectDir)
    {
        var excludedFiles = new List<string>();
        var itemGroups = projectXml.Descendants("ItemGroup");

        foreach (var itemGroup in itemGroups)
        {
            var items = itemGroup.Elements().Where(e => 
                e.Name.LocalName == "Compile" || 
                e.Name.LocalName == "Content");

            foreach (var item in items)
            {
                var remove = item.Attribute("Remove")?.Value;
                if (!string.IsNullOrEmpty(remove))
                {
                    // Handle glob patterns in Remove attributes
                    var pattern = Path.Combine(projectDir, remove);
                    var matchingFiles = GetFilesMatchingPattern(pattern);
                    excludedFiles.AddRange(matchingFiles);
                }
            }
        }

        return excludedFiles;
    }

    /// <summary>
    /// Gets files that are explicitly included in the project file.
    /// </summary>
    private static List<string> GetExplicitlyIncludedFiles(XDocument projectXml, string projectDir, IEnumerable<ISyntaxAnalyzer> analyzers)
    {
        var includedFiles = new List<string>();
        var itemGroups = projectXml.Descendants("ItemGroup");

        foreach (var itemGroup in itemGroups)
        {
            var items = itemGroup.Elements().Where(e => 
                e.Name.LocalName == "Compile" || 
                e.Name.LocalName == "Content");

            foreach (var item in items)
            {
                var include = item.Attribute("Include")?.Value;
                if (!string.IsNullOrEmpty(include))
                {
                    var fullPath = Path.Combine(projectDir, include);
                    if (File.Exists(fullPath) && analyzers.Any(analyzer => analyzer.Capabilities.SupportsFile(fullPath)))
                    {
                        includedFiles.Add(fullPath);
                    }
                }
            }
        }

        return includedFiles;
    }

    /// <summary>
    /// Gets files matching a glob pattern.
    /// </summary>
    private static List<string> GetFilesMatchingPattern(string pattern)
    {
        try
        {
            var directory = Path.GetDirectoryName(pattern);
            var fileName = Path.GetFileName(pattern);
            
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                return new List<string>();

            if (fileName.Contains("*") || fileName.Contains("?"))
            {
                if (Directory.Exists(directory))
                {
                    return Directory.GetFiles(directory, fileName, SearchOption.AllDirectories).ToList();
                }
            }
            else if (File.Exists(pattern))
            {
                return new List<string> { pattern };
            }
        }
        catch
        {
            // Ignore pattern matching errors
        }

        return new List<string>();
    }

    /// <summary>
    /// Checks if a path should be excluded from project analysis.
    /// </summary>
    private static bool IsExcludedPath(string filePath)
    {
        var pathLower = filePath.ToLowerInvariant();
        
        // Common excluded directories
        var excludedDirs = new[] { "\\bin\\", "\\obj\\", "\\packages\\", "\\.git\\", "\\.vs\\", "\\node_modules\\" };
        
        return excludedDirs.Any(dir => pathLower.Contains(dir));
    }

    /// <summary>
    /// Fallback method when project file parsing fails.
    /// Uses directory scanning with common exclusions.
    /// </summary>
    private static List<string> GetFallbackIncludedFiles(string projectDir, IEnumerable<ISyntaxAnalyzer> analyzers)
    {
        var allFiles = Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories)
            .Where(f => !IsExcludedPath(f))
            .ToList();

        return allFiles.Where(file => analyzers.Any(analyzer => analyzer.Capabilities.SupportsFile(file))).ToList();
    }
}
