namespace CSharpAST.Core.Processing;

/// <summary>
/// Parser for Microsoft Visual Studio solution files (.sln)
/// Extracts project file paths and metadata from solution files
/// </summary>
public static class SolutionFileParser
{
    /// <summary>
    /// Extracts all project file paths from a solution file
    /// </summary>
    /// <param name="solutionPath">Path to the .sln file</param>
    /// <returns>List of absolute paths to project files</returns>
    public static List<string> GetProjectFiles(string solutionPath)
    {
        if (!File.Exists(solutionPath))
            throw new FileNotFoundException($"Solution file not found: {solutionPath}");

        var solutionDir = Path.GetDirectoryName(solutionPath) ?? throw new InvalidOperationException("Cannot determine solution directory");
        var projectPaths = new List<string>();
        
        var lines = File.ReadAllLines(solutionPath);
        
        foreach (var line in lines)
        {
            if (line.StartsWith("Project("))
            {
                // Parse project line format: Project("{GUID}") = "ProjectName", "RelativePath", "{ProjectGUID}"
                var projectInfo = ParseProjectLine(line);
                if (projectInfo != null)
                {
                    var absolutePath = Path.GetFullPath(Path.Combine(solutionDir, projectInfo.RelativePath));
                    if (File.Exists(absolutePath) && IsProjectFile(absolutePath))
                    {
                        projectPaths.Add(absolutePath);
                    }
                }
            }
        }

        return projectPaths;
    }

    /// <summary>
    /// Gets solution metadata including name, version, and project count
    /// </summary>
    /// <param name="solutionPath">Path to the .sln file</param>
    /// <returns>Solution metadata</returns>
    public static SolutionInfo GetSolutionInfo(string solutionPath)
    {
        if (!File.Exists(solutionPath))
            throw new FileNotFoundException($"Solution file not found: {solutionPath}");

        var lines = File.ReadAllLines(solutionPath);
        var info = new SolutionInfo
        {
            Name = Path.GetFileNameWithoutExtension(solutionPath),
            Path = solutionPath,
            ProjectFiles = GetProjectFiles(solutionPath)
        };

        // Extract version info
        foreach (var line in lines)
        {
            if (line.StartsWith("Microsoft Visual Studio Solution File, Format Version"))
            {
                var versionPart = line.Substring("Microsoft Visual Studio Solution File, Format Version ".Length);
                info.FormatVersion = versionPart.Trim();
            }
            else if (line.StartsWith("# Visual Studio Version"))
            {
                var versionPart = line.Substring("# Visual Studio Version ".Length);
                info.VisualStudioVersion = versionPart.Trim();
            }
            else if (line.StartsWith("VisualStudioVersion"))
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    info.VisualStudioVersionDetailed = parts[1].Trim();
                }
            }
            else if (line.StartsWith("MinimumVisualStudioVersion"))
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    info.MinimumVisualStudioVersion = parts[1].Trim();
                }
            }
        }

        return info;
    }

    private static ProjectReference? ParseProjectLine(string line)
    {
        try
        {
            // Project line format: Project("{TypeGUID}") = "ProjectName", "RelativePath", "{ProjectGUID}"
            var parts = line.Split('=');
            if (parts.Length != 2) return null;

            var leftPart = parts[0].Trim();
            var rightPart = parts[1].Trim();

            // Extract type GUID from left part
            var typeGuidStart = leftPart.IndexOf('"') + 1;
            var typeGuidEnd = leftPart.IndexOf('"', typeGuidStart);
            if (typeGuidStart == 0 || typeGuidEnd == -1) return null;
            var typeGuid = leftPart.Substring(typeGuidStart, typeGuidEnd - typeGuidStart);

            // Extract project name, path, and GUID from right part
            var rightParts = rightPart.Split(',');
            if (rightParts.Length < 3) return null;

            var name = rightParts[0].Trim().Trim('"');
            var relativePath = rightParts[1].Trim().Trim('"');
            var projectGuid = rightParts[2].Trim().Trim('"');

            return new ProjectReference
            {
                Name = name,
                RelativePath = relativePath,
                ProjectGuid = projectGuid,
                TypeGuid = typeGuid
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool IsProjectFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension == ".csproj" || extension == ".vbproj" || extension == ".fsproj" || extension == ".vcxproj";
    }
}

/// <summary>
/// Information about a solution file
/// </summary>
public class SolutionInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<string> ProjectFiles { get; set; } = new();
    public string? FormatVersion { get; set; }
    public string? VisualStudioVersion { get; set; }
    public string? VisualStudioVersionDetailed { get; set; }
    public string? MinimumVisualStudioVersion { get; set; }
}

/// <summary>
/// Reference to a project within a solution
/// </summary>
public class ProjectReference
{
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ProjectGuid { get; set; } = string.Empty;
    public string TypeGuid { get; set; } = string.Empty;
}
