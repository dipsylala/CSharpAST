using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSharpAST.Core;

public class ASTAnalysis
{
    public string SourceFile { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public ASTNode RootNode { get; set; } = new ASTNode();
}

public class ASTNode
{
    public string Type { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public SourceSpan Span { get; set; } = new SourceSpan();
    public LocationInfo Location { get; set; } = new LocationInfo();
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    public List<ASTNode> Children { get; set; } = new List<ASTNode>();
}

public class LocationInfo
{
    public string Path { get; set; } = string.Empty;
    public LinePositionSpan Span { get; set; } = new LinePositionSpan();
    public bool HasMappedPath { get; set; }
}

public class SourceSpan
{
    public int Start { get; set; }
    public int End { get; set; }
    public int Length { get; set; }
}

public class LinePositionSpan
{
    public LinePosition Start { get; set; }
    public LinePosition End { get; set; }
}

public class LinePosition
{
    public int Line { get; set; }
    public int Character { get; set; }
}

public class AsyncMethodInfo
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new List<string>();
    public List<string> AwaitExpressions { get; set; } = new List<string>();
    public int LineNumber { get; set; }
}

public class ClassInfo
{
    public string Name { get; set; } = string.Empty;
    public List<string> Modifiers { get; set; } = new List<string>();
    public List<string> BaseTypes { get; set; } = new List<string>();
    public List<string> Methods { get; set; } = new List<string>();
    public List<string> Properties { get; set; } = new List<string>();
    public int LineNumber { get; set; }
}

public class InterfaceInfo
{
    public string Name { get; set; } = string.Empty;
    public List<string> Methods { get; set; } = new List<string>();
    public List<string> Properties { get; set; } = new List<string>();
    public int LineNumber { get; set; }
}

public class PropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsAutoImplemented { get; set; }
    public int LineNumber { get; set; }
}

// Project and Solution Analysis Models
public class ProjectAnalysis
{
    public string ProjectPath { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public List<ASTAnalysis> Files { get; set; } = new List<ASTAnalysis>();
    public List<string> Dependencies { get; set; } = new List<string>();
    public List<ClassInfo> TestClasses { get; set; } = new List<ClassInfo>();
    public List<AsyncPatternInfo> AsyncPatterns { get; set; } = new List<AsyncPatternInfo>();
    public object? TestData { get; set; }
}

public class SolutionAnalysis
{
    public string SolutionPath { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public List<ProjectAnalysis> Projects { get; set; } = new List<ProjectAnalysis>();
    public object? TestData { get; set; }
}

public class AsyncPatternInfo
{
    public string MethodName { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public int AwaitCount { get; set; }
    public bool HasConfigureAwait { get; set; }
    public bool HasTaskWhenAll { get; set; }
    public bool HasTaskWhenAny { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

// Additional classes needed for integration tests
public class UsingDirectiveInfo
{
    public string Namespace { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public int LineNumber { get; set; }
}

public class FileAnalysis
{
    public string FilePath { get; set; } = string.Empty;
    public List<ClassInfo> Classes { get; set; } = new List<ClassInfo>();
    public List<InterfaceInfo> Interfaces { get; set; } = new List<InterfaceInfo>();
    public List<string> Methods { get; set; } = new List<string>();
    public List<string> Enums { get; set; } = new List<string>();
    public List<string> Properties { get; set; } = new List<string>();
}
