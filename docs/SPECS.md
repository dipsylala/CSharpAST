# CSharpAST Technical Specifications

This document provides comprehensive technical specifications for the CSharpAST system, designed to enable Large Language Models (LLMs) to quickly understand the codebase structure, functionality, and implementation details.

## Table of Contents

1. [System Overview](#system-overview)
2. [Core Data Models](#core-data-models)
3. [Interface Specifications](#interface-specifications)
4. [Implementation Details](#implementation-details)
5. [API Reference](#api-reference)
6. [Output Formats](#output-formats)
7. [Performance Metrics](#performance-metrics)
8. [Error Handling](#error-handling)
9. [Configuration](#configuration)
10. [Testing Specifications](#testing-specifications)

## System Overview

### Purpose
CSharpAST is a high-performance Abstract Syntax Tree (AST) generation and analysis tool for C# source code, built on Microsoft's Roslyn compiler APIs. It provides deep code analysis, async pattern detection, and automated test generation capabilities.

### Key Capabilities
- **Deep AST Generation**: Complete syntax tree generation with node-level details for C#, VB.NET, and Razor files
- **Multi-Language Support**: Unified processing of C#, VB.NET, and Razor/CSHTML files with intelligent analyzer selection
- **MVC Pattern Support**: AST analysis of ASP.NET MVC components including controllers, views, and Razor rendering
- **Async Pattern Detection**: Comprehensive analysis of async/await patterns and task-based operations
- **Cross-File Analysis**: Project and solution-level analysis with dependency tracking
- **Performance Optimization**: Concurrent processing with intelligent load balancing

### Roadmap Features
- **Data Flow Analysis**: End-to-end tracking of data flow from user input through controllers to view output (*planned*)
- **Security Analysis**: Automated detection of security vulnerabilities and best practices (*planned*)
- **Advanced Cross-Reference Analysis**: Deep relationship mapping between controllers, views, models, and services (*planned*)

### Technology Stack
- **.NET 8.0**: Target framework for all projects
- **Microsoft.CodeAnalysis.CSharp 4.8.0+**: Roslyn compiler APIs
- **Newtonsoft.Json**: JSON serialization
- **System.CommandLine**: CLI argument parsing
- **NUnit**: Unit testing framework
- **System.Threading.Tasks**: Async/await operations

### Project Structure
```
CSharpAST/                              # Solution root
├── CSharpAST.Core/                     # Core analysis engine
│   ├── Analysis/                       # Syntax analyzers
│   │   ├── ISyntaxAnalyzer.cs         # Core analyzer interface
│   │   ├── SyntaxAnalyzer.cs          # Standard implementation
│   │   └── CSharpSyntaxAnalyzer.cs # High-performance implementation
│   ├── Processing/                     # File/project processors
│   │   ├── IFileProcessor.cs          # File processing interface
│   │   ├── UnifiedFileProcessor.cs    # Multi-language implementation  
│   │   └── ConcurrentFileProcessor.cs # Multi-threaded implementation
│   ├── Output/                         # Output formatters
│   │   ├── IOutputManager.cs          # Output interface
│   │   └── OutputManager.cs           # Multi-format implementation
│   ├── ASTGenerator.cs                # Main orchestration class with concurrent processing
│   └── Models.cs                      # Data transfer objects
├── CSharpAST.CLI/                     # Command-line interface
├── CSharpAST.TestGeneration/          # Test scaffolding generator
├── CSharpAST.Performance/             # Performance benchmarking
├── CSharpAST.PerformanceTest/         # Performance test runner
└── CSharpAST.IntegrationTests/        # Integration test suite
```

## Core Data Models

### ASTAnalysis
Primary data structure containing complete analysis results.

```csharp
public class ASTAnalysis
{
    public string SourceFile { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public ASTNode RootNode { get; set; } = new ASTNode();
    public List<AsyncMethodInfo> AsyncMethods { get; set; } = new List<AsyncMethodInfo>();
    public List<ClassInfo> Classes { get; set; } = new List<ClassInfo>();
    public List<InterfaceInfo> Interfaces { get; set; } = new List<InterfaceInfo>();
    public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
    public List<UsingDirectiveInfo> UsingDirectives { get; set; } = new List<UsingDirectiveInfo>();
}
```

### ASTNode
Hierarchical representation of syntax tree nodes.

```csharp
public class ASTNode
{
    public string Type { get; set; } = string.Empty;           // C# syntax node type
    public string Kind { get; set; } = string.Empty;           // SyntaxKind enumeration
    public string Text { get; set; } = string.Empty;           // Source code text
    public Dictionary<string, object> Properties { get; set; } = new();    // Node metadata
    public List<ASTNode> Children { get; set; } = new List<ASTNode>();     // Child nodes
    public LocationInfo Location { get; set; } = new LocationInfo();       // Source location
}
```

### LocationInfo
Source code position information.

```csharp
public class LocationInfo
{
    public string Path { get; set; } = string.Empty;
    public LinePositionSpan Span { get; set; } = new LinePositionSpan();
    public bool HasMappedPath { get; set; }
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
```

### AsyncMethodInfo
Specialized analysis for async methods.

```csharp
public class AsyncMethodInfo
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new List<string>();
    public List<string> AwaitExpressions { get; set; } = new List<string>();
    public bool UsesConfigureAwait { get; set; }
    public int LineNumber { get; set; }
}
```

### ProjectAnalysis
Project-level analysis aggregation.

```csharp
public class ProjectAnalysis
{
    public string ProjectPath { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<ASTAnalysis> Files { get; set; } = new List<ASTAnalysis>();
    public List<string> Dependencies { get; set; } = new List<string>();
    public List<ClassInfo> TestClasses { get; set; } = new List<ClassInfo>();
    public List<AsyncPatternInfo> AsyncPatterns { get; set; } = new List<AsyncPatternInfo>();
}
```

## Roadmap Data Models

### DataFlowAnalysis (*Planned Feature*)
Comprehensive data flow tracking for MVC applications - part of the planned roadmap.

```csharp
public class DataFlowAnalysis
{
    public string RequestPath { get; set; } = string.Empty;                          // HTTP route path
    public List<DataFlowSegment> FlowSegments { get; set; } = new List<DataFlowSegment>();
    public List<UserInputPoint> InputPoints { get; set; } = new List<UserInputPoint>();
    public List<ViewOutputPoint> OutputPoints { get; set; } = new List<ViewOutputPoint>();
    public List<DataTransformation> Transformations { get; set; } = new List<DataTransformation>();
    public List<SecurityCheckpoint> SecurityChecks { get; set; } = new List<SecurityCheckpoint>();
}

public class DataFlowSegment
{
    public string SegmentType { get; set; } = string.Empty;    // "UserInput", "Controller", "Service", "View", "Output"
    public string SourceFile { get; set; } = string.Empty;     // File containing this segment
    public string Method { get; set; } = string.Empty;         // Method or action name
    public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

public class DataPoint
{
    public string Name { get; set; } = string.Empty;           // Variable/property name
    public string Type { get; set; } = string.Empty;           // Data type
    public string Source { get; set; } = string.Empty;         // Where data comes from
    public string Destination { get; set; } = string.Empty;    // Where data goes to
    public List<string> ValidationRules { get; set; } = new List<string>();
    public bool IsUserInput { get; set; }                      // Directly from user
    public bool IsUserOutput { get; set; }                     // Directly to user
    public int LineNumber { get; set; }
}

public class UserInputPoint
{
    public string ParameterName { get; set; } = string.Empty;
    public string ParameterType { get; set; } = string.Empty;
    public string BindingSource { get; set; } = string.Empty;  // "Body", "Query", "Route", "Form"
    public List<string> ValidationAttributes { get; set; } = new List<string>();
    public string ControllerAction { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

public class ViewOutputPoint
{
    public string ViewPath { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public List<string> DisplayedProperties { get; set; } = new List<string>();
    public List<string> UserInteractionElements { get; set; } = new List<string>();  // Forms, links, buttons
    public int LineNumber { get; set; }
}

public class DataTransformation
{
    public string TransformationType { get; set; } = string.Empty;  // "Mapping", "Validation", "Formatting", "Calculation"
    public string SourceType { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Logic { get; set; } = string.Empty;              // Transformation logic/expression
    public int LineNumber { get; set; }
}

public class SecurityCheckpoint 
{
    public string CheckType { get; set; } = string.Empty;          // "Authorization", "Validation", "Sanitization"
    public string Method { get; set; } = string.Empty;
    public List<string> Rules { get; set; } = new List<string>();
    public bool IsApplied { get; set; }
    public int LineNumber { get; set; }
}
```

## Interface Specifications

### ISyntaxAnalyzer
Core interface for syntax tree analysis.

```csharp
public interface ISyntaxAnalyzer
{
    /// <summary>
    /// Analyzes a complete syntax tree and generates comprehensive AST analysis
    /// </summary>
    /// <param name="root">Root syntax node from Roslyn</param>
    /// <param name="filePath">Source file path for context</param>
    /// <returns>Complete AST analysis with metadata</returns>
    ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath);

    /// <summary>
    /// Analyzes a single syntax node and its immediate children
    /// </summary>
    /// <param name="node">Syntax node to analyze</param>
    /// <returns>AST node representation</returns>
    ASTNode AnalyzeNode(SyntaxNode node);
}
```

**Implementations:**
- `SyntaxAnalyzer`: Standard sequential implementation
- `CSharpSyntaxAnalyzer`: High-performance with memory pooling and conditional parallelization

### IFileProcessor
Interface for processing various file types.

```csharp
public interface IFileProcessor : IDisposable
{
    /// <summary>
    /// Process a single C# file asynchronously
    /// </summary>
    Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a C# project file (.csproj)
    /// </summary>
    Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a solution file (.sln)
    /// </summary>
    Task<ASTAnalysis?> ProcessSolutionAsync(string solutionPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process multiple files concurrently
    /// </summary>
    Task<List<ASTAnalysis>> ProcessMultipleFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file type is supported
    /// </summary>
    bool IsFileSupported(string filePath);
}
```

**Implementations:**
- `UnifiedFileProcessor`: Multi-language processor supporting C#, VB.NET, and Razor files with intelligent analyzer selection
- `ConcurrentFileProcessor`: Multi-threaded with semaphore-based concurrency control

### IOutputManager
Interface for output formatting and file writing.

```csharp
public interface IOutputManager
{
    /// <summary>
    /// Write analysis results in specified format
    /// </summary>
    Task WriteAsync(ASTAnalysis analysis, string outputPath, string format);
    Task WriteAsync(ProjectAnalysis analysis, string outputPath, string format);

    /// <summary>
    /// Get file extension for format
    /// </summary>
    string GetFileExtension(string format);

    /// <summary>
    /// Check if format is supported
    /// </summary>
    bool IsFormatSupported(string format);

    /// <summary>
    /// Validate output path accessibility
    /// </summary>
    bool CanWriteToPath(string outputPath);
}
```

**Supported Formats:**
- `json`: Structured JSON output (default)
- `xml`: XML format for enterprise integration
- `text`: Human-readable text summaries

## Implementation Details

### Standard ASTGenerator
Main orchestration class for AST generation.

```csharp
public class ASTGenerator
{
    private readonly ISyntaxAnalyzer _syntaxAnalyzer;
    private readonly IFileProcessor _fileProcessor;
    private readonly IOutputManager _outputManager;
    private readonly bool _verbose;

    // Constructor with dependency injection
    public ASTGenerator(ISyntaxAnalyzer syntaxAnalyzer, IFileProcessor fileProcessor, 
                       IOutputManager outputManager, bool verbose = false);

    // Main entry point for AST generation
    public async Task GenerateASTAsync(string inputPath, string outputPath, string format);

    // Legacy methods for backward compatibility
    public async Task<ASTAnalysis?> GenerateFromFileAsync(string filePath);
    public async Task<ProjectAnalysis?> GenerateFromProjectAsync(string projectPath);
}
```

### ASTGenerator Processing Modes
The ASTGenerator supports configurable processing modes for different performance requirements.

```csharp
public class ASTGenerator : IDisposable
{
    public enum ProcessingMode
    {
        Unified,      // Sequential processing, supports all file types
        Concurrent    // Concurrent processing, supports all file types
    }

    // Constructor with processing mode selection
    public ASTGenerator(ProcessingMode mode = ProcessingMode.Concurrent, 
                       bool verbose = false, int? maxConcurrency = null);
    
    // Factory method for sequential processing
    public static ASTGenerator CreateUnified(bool verbose = false, int? maxConcurrency = null);
    
    // Main analysis methods
    public async Task GenerateASTAsync(string inputPath, string outputPath, string format);
    public async Task<ASTAnalysis?> GenerateFromFileAsync(string filePath);
    public async Task<ProjectAnalysis?> GenerateFromProjectAsync(string projectPath);
}
```

### Performance Characteristics

| Component | Sequential | Concurrent | Improvement |
|-----------|------------|------------|-------------|
| Single File (5KB) | 8-17ms | 1-8ms | 2-3x faster |
| Multiple Files | Sequential | Concurrent | 2-4x faster |
| Project Analysis | 115ms | 5ms | 23x faster |
| Memory Usage | Standard | Optimized batching | Improved efficiency |
| CPU Utilization | Single core | Multi-core | Scales with cores |

### Concurrency Model

#### ConcurrentFileProcessor
```csharp
public class ConcurrentFileProcessor : IFileProcessor
{
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly int _maxConcurrency;

    // Intelligent concurrency calculation
    // Default: Environment.ProcessorCount * 2, max 16
    public ConcurrentFileProcessor(ISyntaxAnalyzer syntaxAnalyzer, int? maxConcurrency = null);

    // Semaphore-controlled concurrent processing
    public async Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, 
                                                           CancellationToken cancellationToken = default);
}
```

#### CSharpSyntaxAnalyzer

```csharp
public class CSharpSyntaxAnalyzer : ISyntaxAnalyzer
{
    // Memory pools for object reuse
    private readonly ArrayPool<ASTNode> _nodePool;
    private readonly ConcurrentQueue<List<ASTNode>> _listPool;
    private readonly ConcurrentQueue<Dictionary<string, object>> _dictPool;

    // Conditional parallelization thresholds
    private const int PARALLEL_CHILDREN_THRESHOLD = 50;
    private const int PARALLEL_CONSTRUCTS_THRESHOLD = 1000;

    // Smart parallel processing
    public ASTNode AnalyzeNode(SyntaxNode node);
}
```

## API Reference

### Core API Methods

#### ASTGenerator.GenerateASTAsync()
```csharp
/// <summary>
/// Primary method for generating AST analysis from various input sources
/// </summary>
/// <param name="inputPath">File, directory, project, or solution path</param>
/// <param name="outputPath">Output directory for generated files</param>
/// <param name="format">Output format: "json", "xml", or "text"</param>
/// <returns>Task representing the async operation</returns>
public async Task GenerateASTAsync(string inputPath, string outputPath, string format)
```

**Input Types Supported:**
- `.cs` files: Individual C# source files
- Directories: All `.cs` files recursively (excluding bin/obj)
- `.csproj` files: Complete project analysis
- `.sln` files: Solution-wide analysis

**Output Structure:**
- Single file: `{filename}_ast.{extension}`
- Project: `{projectname}_project_ast.{extension}`
- Solution: `{solutionname}_solution_ast.{extension}`

#### Performance Benchmarking
```csharp
/// <summary>
/// Compare sequential vs concurrent performance
/// </summary>
public async Task<BenchmarkSummary> RunBenchmarksAsync(string testPath, int iterations = 3);

public class BenchmarkSummary
{
    public double OverallSpeedupFactor { get; set; }    // Average speedup across all tests
    public double BestSpeedupFactor { get; set; }       // Best case performance improvement
    public double WorstSpeedupFactor { get; set; }      // Worst case performance
    public double TotalStandardTime { get; set; }       // Total time for standard implementation
    public double TotalOptimizedTime { get; set; }      // Total time for optimized implementation
}
```

### Command Line Interface

#### Program Entry Point
```bash
dotnet run --project CSharpAST.CLI -- [options]
```

#### Arguments Specification
```
--input, -i     : Input path (required)
                  Accepts: .cs files, directories, .csproj files, .sln files
                  
--output, -o    : Output directory (required)
                  Must be writable path, will be created if not exists
                  
--format, -f    : Output format (optional, default: json)
                  Values: json, xml, text
                  
--verbose, -v   : Enable verbose logging (optional, default: false)
                  Provides detailed progress and performance information
```

## Output Formats

### JSON Format Structure
```json
{
  "sourceFile": "path/to/source.cs",
  "generatedAt": "2025-08-03T10:30:00Z",
  "rootNode": {
    "type": "CompilationUnitSyntax",
    "kind": "CompilationUnit",
    "text": "using System;...",
    "properties": {
      "fileName": "source.cs",
      "encoding": "utf-8"
    },
    "children": [
      {
        "type": "UsingDirectiveSyntax",
        "kind": "UsingDirective",
        "properties": {
          "namespace": "System",
          "isStatic": false
        }
      }
    ],
    "location": {
      "path": "source.cs",
      "span": {
        "start": { "line": 0, "character": 0 },
        "end": { "line": 10, "character": 0 }
      }
    }
  },
  "asyncMethods": [
    {
      "name": "ProcessAsync",
      "returnType": "Task<string>",
      "parameters": ["string input"],
      "awaitExpressions": ["_service.GetDataAsync(input)"],
      "usesConfigureAwait": true,
      "lineNumber": 15
    }
  ],
  "classes": [
    {
      "name": "DataProcessor",
      "namespace": "MyProject.Services",
      "modifiers": ["public"],
      "baseTypes": ["IDataProcessor"],
      "methods": ["ProcessAsync", "ValidateInput"],
      "properties": ["IsEnabled", "Timeout"]
    }
  ]
}
```

### Data Flow Analysis JSON Example (*Planned Feature*)
For MVC applications, the tool will be able to generate comprehensive data flow analysis:

> **Note**: This feature is part of the planned roadmap and is not yet implemented. The example below shows the intended output format for future data flow analysis capabilities.

```json
{
  "dataFlowAnalysis": {
    "requestPath": "/Books/Index",
    "flowSegments": [
      {
        "segmentType": "UserInput",
        "sourceFile": "Controllers/BooksController.cs",
        "method": "Index",
        "dataPoints": [
          {
            "name": "searchTerm",
            "type": "string",
            "source": "QueryString",
            "destination": "BookService.SearchBooksAsync",
            "validationRules": [],
            "isUserInput": true,
            "lineNumber": 23
          }
        ]
      },
      {
        "segmentType": "Controller",
        "sourceFile": "Controllers/BooksController.cs", 
        "method": "Index",
        "dataPoints": [
          {
            "name": "books",
            "type": "IEnumerable<Book>",
            "source": "BookService.SearchBooksAsync",
            "destination": "BookListViewModel.Books",
            "lineNumber": 33
          },
          {
            "name": "viewModel",
            "type": "BookListViewModel",
            "source": "Controller Logic",
            "destination": "View",
            "lineNumber": 48
          }
        ]
      },
      {
        "segmentType": "View",
        "sourceFile": "Views/Books/Index.cshtml",
        "method": "RenderView",
        "dataPoints": [
          {
            "name": "Model.Books",
            "type": "IEnumerable<Book>",
            "source": "ViewModel",
            "destination": "HTML Output",
            "isUserOutput": true,
            "lineNumber": 85
          }
        ]
      }
    ],
    "inputPoints": [
      {
        "parameterName": "searchTerm",
        "parameterType": "string",
        "bindingSource": "Query",
        "validationAttributes": [],
        "controllerAction": "BooksController.Index",
        "lineNumber": 23
      }
    ],
    "outputPoints": [
      {
        "viewPath": "Views/Books/Index.cshtml",
        "modelType": "BookListViewModel",
        "displayedProperties": ["Books", "SearchTerm", "TotalBooks"],
        "userInteractionElements": ["SearchForm", "GenreFilter", "PaginationLinks"],
        "lineNumber": 85
      }
    ],
    "transformations": [
      {
        "transformationType": "Mapping",
        "sourceType": "IEnumerable<Book>",
        "targetType": "BookListViewModel",
        "method": "BooksController.Index",
        "logic": "new BookListViewModel { Books = pagedBooks, SearchTerm = searchTerm }",
        "lineNumber": 48
      }
    ],
    "securityChecks": [
      {
        "checkType": "Authorization",
        "method": "BooksController",
        "rules": ["Authenticated Users"],
        "isApplied": true,
        "lineNumber": 10
      }
    ]
  }
}
```

### XML Format Structure
```xml
<?xml version="1.0" encoding="utf-8"?>
<ASTAnalysis>
  <SourceFile>path/to/source.cs</SourceFile>
  <GeneratedAt>2025-08-03T10:30:00Z</GeneratedAt>
  <RootNode type="CompilationUnitSyntax" kind="CompilationUnit">
    <Properties>
      <Property name="fileName" value="source.cs" />
    </Properties>
    <Children>
      <Node type="UsingDirectiveSyntax" kind="UsingDirective">
        <Properties>
          <Property name="namespace" value="System" />
        </Properties>
      </Node>
    </Children>
  </RootNode>
  <AsyncMethods>
    <AsyncMethod name="ProcessAsync" returnType="Task&lt;string&gt;">
      <Parameters>
        <Parameter>string input</Parameter>
      </Parameters>
      <AwaitExpressions>
        <Expression>_service.GetDataAsync(input)</Expression>
      </AwaitExpressions>
    </AsyncMethod>
  </AsyncMethods>
</ASTAnalysis>
```

### Text Format Structure
```text
AST Analysis for: path/to/source.cs
Generated at: 2025-08-03 10:30:00 UTC

=== SYNTAX TREE ===
CompilationUnit - CompilationUnitSyntax
  UsingDirective - UsingDirectiveSyntax
    Text: using System;
  NamespaceDeclaration - NamespaceDeclarationSyntax
    ClassDeclaration - ClassDeclarationSyntax
      Text: public class DataProcessor

=== ASYNC METHODS ===
ProcessAsync
  Return Type: Task<string>
  Parameters: string input
  Await Expressions: _service.GetDataAsync(input)
  ConfigureAwait Usage: Yes
  Line: 15

=== CLASSES ===
DataProcessor
  Namespace: MyProject.Services
  Modifiers: public
  Base Types: IDataProcessor
  Methods: ProcessAsync, ValidateInput
  Properties: IsEnabled, Timeout
```

## Performance Metrics

### Benchmark Categories

#### Single File Processing
- **Small Files** (< 1KB): Minimal improvement, overhead consideration
- **Medium Files** (1-10KB): 2-3x speedup with optimizations
- **Large Files** (> 10KB): 3-5x speedup with parallel node processing

#### Multi-File Processing
- **Sequential Baseline**: Standard implementation processes files one by one
- **Concurrent Optimized**: Processes multiple files simultaneously
- **Optimal Thread Count**: 4-8 threads for most scenarios

#### Project-Level Processing
- **Sequential Implementation**: 100-200ms for typical projects
- **Concurrent Implementation**: 5-15ms for same projects
- **Speedup Factor**: 10-40x improvement depending on project size

### Performance Configuration

#### Concurrency Settings
```csharp
// Default concurrency calculation
int defaultConcurrency = Math.Min(Environment.ProcessorCount * 2, 16);

// Custom concurrency for concurrent processing
var generator = new ASTGenerator(ASTGenerator.ProcessingMode.Concurrent, maxConcurrency: 8);

// Sequential processing when needed
var sequentialGenerator = ASTGenerator.CreateUnified();
```

// Pool size thresholds
private const int MAX_POOLED_LIST_SIZE = 100;
private const int MAX_POOLED_DICT_SIZE = 100;
```

## Error Handling

### Error Categories

#### File System Errors
- **File Not Found**: Graceful handling with user-friendly messages
- **Access Denied**: Permission validation with helpful guidance
- **Path Too Long**: Path length validation and truncation suggestions

#### Parsing Errors
- **Syntax Errors**: Roslyn compiler error reporting
- **Encoding Issues**: Automatic encoding detection and fallback
- **Malformed Code**: Partial analysis with error reporting

#### Runtime Errors
- **Out of Memory**: Memory usage monitoring and optimization suggestions
- **Thread Exhaustion**: Concurrency limiting and resource management
- **Timeout Errors**: Cancellation token support and timeout handling

### Error Response Format
```json
{
  "success": false,
  "error": {
    "type": "FileProcessingError",
    "message": "Unable to process file: MyClass.cs",
    "details": "The file contains syntax errors that prevent parsing",
    "filePath": "src/MyClass.cs",
    "lineNumber": 15,
    "suggestion": "Fix syntax errors and try again"
  },
  "partialResults": {
    "processedFiles": 5,
    "failedFiles": 1,
    "totalFiles": 6
  }
}
```

## Configuration

### Environment Variables
```bash
CSHARPAST_MAX_CONCURRENCY=8        # Override default concurrency
CSHARPAST_MEMORY_LIMIT=2048        # Memory limit in MB
CSHARPAST_TIMEOUT=300               # Timeout in seconds
CSHARPAST_LOG_LEVEL=Information     # Logging level
```

### Configuration File Support
```json
{
  "performance": {
    "maxConcurrency": 8,
    "enableMemoryPooling": true,
    "parallelThreshold": 50
  },
  "output": {
    "defaultFormat": "json",
    "includeSourceText": false,
    "compressOutput": true
  },
  "analysis": {
    "includeAsyncPatterns": true,
    "detectTestClasses": true,
    "analyzeMetadata": true
  }
}
```

## Testing Specifications

### Unit Test Coverage
- **Core Components**: 95%+ coverage for all interfaces and implementations
- **Edge Cases**: Comprehensive testing of error conditions and edge cases
- **Performance Tests**: Benchmark validation and regression detection

### Integration Test Scenarios
1. **Single File Analysis**: Verify complete AST generation for individual files
2. **Project Analysis**: Validate project-level aggregation and dependency detection
3. **Solution Analysis**: Test cross-project analysis and reporting
4. **Performance Regression**: Ensure optimizations maintain performance gains
5. **Error Recovery**: Validate graceful handling of various error conditions

### Test Data
- **Sample Projects**: Curated test projects with known characteristics
- **Synthetic Code**: Generated code samples for specific pattern testing
- **Real-World Projects**: Analysis of actual open-source projects
- **Edge Cases**: Malformed code, empty files, large files, binary files

### Performance Benchmarks
```csharp
[Test]
[Performance]
public async Task ConcurrentGenerator_MaintainsPerformance_OnLargeProjects()
{
    var generator = new ASTGenerator(ASTGenerator.ProcessingMode.Concurrent);
    var stopwatch = Stopwatch.StartNew();
    
    var result = await generator.GenerateFromProjectAsync("LargeProject.csproj");
    stopwatch.Stop();
    
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(20), 
        "Large project analysis should complete within 20ms");
    Assert.That(result.Files.Count, Is.GreaterThan(50), 
        "Should process all files in large project");
}
```

---

This specification document provides comprehensive technical details for understanding, implementing, and extending the CSharpAST system. All interfaces, data models, and behaviors are designed to be deterministic and testable, making the system suitable for automated analysis and integration into larger development workflows.
