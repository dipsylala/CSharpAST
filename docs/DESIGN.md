# CSharpAST Design Document

This document provides a comprehensive technical design overview of the CSharpAST system, intended for developers who need to understand, maintain, or extend the codebase.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Component Design](#component-design)
3. [Data Flow](#data-flow)
4. [Performance Optimizations](#performance-optimizations)
5. [Extensibility Points](#extensibility-points)
6. [Error Handling Strategy](#error-handling-strategy)
7. [Testing Strategy](#testing-strategy)
8. [Threading Model](#threading-model)

## Architecture Overview

### Design Principles

The CSharpAST system is built on the following core principles:

1. **Separation of Concerns**: Each component has a single, well-defined responsibility
2. **Dependency Injection**: All components are loosely coupled through interfaces
3. **Performance-First**: Multi-threaded processing with intelligent optimization
4. **Extensibility**: Plugin-based architecture for analyzers and output formats
5. **Robustness**: Comprehensive error handling and graceful degradation

### High-Level Architecture

```
┌─────────────────┐      ┌─────────────────┐     ┌─────────────────┐
│   Presentation  │      │    Application  │     │   Performance   │
│     Layer       │      │     Layer       │     │     Layer       │
│                 │      │                 │     │                 │
│ • CLI Interface │────▶│ • ASTGenerator  │◀────│ • Benchmarking  │
│ • Argument      │      │ • Orchestration │     │ • Optimization  │
│   Parsing       │      │ • Coordination  │     │ • Metrics       │
└─────────────────┘      └─────────┬───────┘     └─────────────────┘
                                   │
                         ┌─────────▼───────┐
                         │   Core Domain   │
                         │     Layer       │
                         │                 │
                         │ • Analysis      │
                         │ • Processing    │
                         │ • Output        │
                         │ • Models        │
                         └─────────────────┘
```

### Layer Responsibilities

#### Presentation Layer
- **CSharpAST.CLI**: Command-line interface and user interaction
- **Argument Processing**: Input validation and parameter parsing
- **User Feedback**: Progress reporting and error messaging

#### Application Layer
- **ASTGenerator**: Unified processing orchestration with standard and optimized modes
- **Workflow Coordination**: Managing the overall analysis pipeline

#### Performance Layer
- **CSharpAST.Performance**: Benchmarking and performance analysis
- **CSharpAST.PerformanceTest**: Performance testing applications
- **Optimization Strategies**: Concurrent processing and memory management

#### Core Domain Layer
- **Analysis**: Syntax analysis and AST generation
- **Processing**: File, project, and solution processing
- **Output**: Formatting and serialization
- **Models**: Data transfer objects and domain models

## Component Design

### Core Components

#### 1. ISyntaxAnalyzer Interface

```csharp
public interface ISyntaxAnalyzer
{
    ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath);
    ASTNode AnalyzeNode(SyntaxNode node);
}
```

**Implementations:**
- `SyntaxAnalyzer`: Standard implementation with basic optimizations
- `OptimizedSyntaxAnalyzer`: High-performance implementation with memory pooling

**Design Decisions:**
- Interface allows for swappable analysis strategies
- Node-level analysis enables granular control
- Memory pooling reduces GC pressure in high-throughput scenarios

#### 2. IFileProcessor Interface

```csharp
public interface IFileProcessor : IDisposable
{
    Task<ASTAnalysis?> ProcessFileAsync(string filePath);
    Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<ASTAnalysis?> ProcessProjectAsync(string projectPath, CancellationToken cancellationToken = default);
    Task<ASTAnalysis?> ProcessSolutionAsync(string solutionPath, CancellationToken cancellationToken = default);
    Task<List<ASTAnalysis>> ProcessMultipleFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default);
    bool IsFileSupported(string filePath);
}
```

**Implementations:**
- `FileProcessor`: Sequential file processing with basic error handling
- `ConcurrentFileProcessor`: Multi-threaded processing with concurrency limiting

**Design Decisions:**
- Async processing for I/O-bound operations
- Cancellation support for long-running operations
- File type validation prevents invalid processing
- IDisposable pattern for resource cleanup

#### 3. IOutputManager Interface

```csharp
public interface IOutputManager
{
    Task WriteOutputAsync(ASTAnalysis analysis, string outputPath);
    Task WriteAsync(ASTAnalysis analysis, string outputPath, string format);
    Task WriteAsync(ProjectAnalysis analysis, string outputPath, string format);
    string GetOutputExtension();
    string GetFileExtension(string format);
    bool IsFormatSupported(string format);
    bool CanWriteToPath(string outputPath);
}
```

**Implementation:**
- `OutputManager`: Multi-format output with JSON, XML, and Text support

**Design Decisions:**
- Format-agnostic interface enables easy extension
- Path validation prevents runtime failures
- Async I/O for non-blocking file operations

### Advanced Components

#### 1. OptimizedSyntaxAnalyzer

**Key Features:**
- **Memory Pooling**: Uses `ArrayPool<T>` and `ConcurrentQueue<T>` for object reuse
- **Conditional Parallelization**: Only uses parallel processing when beneficial
- **Smart Thresholds**: Different processing strategies based on tree size

**Implementation Details:**
```csharp
private readonly ArrayPool<ASTNode> _nodePool;
private readonly ConcurrentQueue<List<ASTNode>> _listPool;
private readonly ConcurrentQueue<Dictionary<string, object>> _dictPool;

// Conditional parallel processing
if (node.ChildNodes().Count() > 50)
{
    children = node.ChildNodes()
        .AsParallel()
        .AsOrdered()
        .Select(child => AnalyzeNode(child))
        .ToList();
}
```

#### 2. ConcurrentFileProcessor

**Key Features:**
- **Semaphore-Based Concurrency Limiting**: Prevents thread pool exhaustion
- **Partitioned Processing**: Load balancing across available cores
- **Error Isolation**: Per-file error handling prevents cascade failures

**Implementation Details:**
```csharp
private readonly SemaphoreSlim _concurrencyLimiter;
private readonly int _maxConcurrency;

// Intelligent concurrency calculation
_maxConcurrency = maxConcurrency ?? Math.Min(Environment.ProcessorCount * 2, 16);

// Partitioned parallel processing
await Parallel.ForEachAsync(Partitioner.Create(filePaths, true), new ParallelOptions
{
    MaxDegreeOfParallelism = _maxConcurrency
}, async (filePath, ct) => { /* processing logic */ });
```

## Data Flow

### Standard Processing Flow

```
Input Source
     │
     ▼
┌─────────────────┐
│ File Detection  │ ──┐
│ • .cs files     │   │
│ • .csproj files │   │
│ • .sln files    │   │
└─────────────────┘   │
     │                │
     ▼                │
┌─────────────────┐   │
│  File Reading   │   │
│ • Async I/O     │   │
│ • Encoding      │   │
│ • Error Handling│   │
└─────────────────┘   │
     │                │
     ▼                │
┌─────────────────┐   │
│ Syntax Parsing  │   │
│ • Roslyn APIs   │   │
│ • Tree Creation │   │
│ • Validation    │   │
└─────────────────┘   │
     │                │
     ▼                │
┌─────────────────┐   │
│ AST Analysis    │   │ Error
│ • Node Walking  │   │ Recovery
│ • Pattern Detect│   │ & Logging
│ • Metadata      │   │    │
└─────────────────┘   │    │
     │                │    │
     ▼                │    │
┌─────────────────┐   │    │
│ Data Assembly   │   │    │
│ • ASTAnalysis   │ ◀─┘    │
│ • Aggregation   │        │
│ • Validation    │        │
└─────────────────┘        │
     │                     │
     ▼                     │
┌─────────────────┐        │
│ Output Writing  │        │
│ • Serialization │        │
│ • File Creation │ ◀──────┘
│ • Format Select │
└─────────────────┘
     │
     ▼
Output Files
```

### Optimized Processing Flow

```
Input Source
     │
     ▼
┌─────────────────┐
│ Batching &      │
│ Load Balancing  │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Thread Pool   │     │   Thread Pool   │     │   Thread Pool   │
│     Worker      │     │     Worker      │     │     Worker      │
│                 │     │                 │     │                 │
│ ┌─────────────┐ │     │ ┌─────────────┐ │     │ ┌─────────────┐ │
│ │ File Proc.  │ │ ┌──▶│ │ File Proc.  │ │ ┌──▶│ │ File Proc.  │ │
│ │ + Analysis  │ │ │   │ │ + Analysis  │ │ │   │ │ + Analysis  │ │
│ └─────────────┘ │ │   │ └─────────────┘ │ │   │ └─────────────┘ │
└─────────┬───────┘ │   └─────────┬───────┘ │   └─────────┬───────┘
          │         │             │         │             │
          ▼         │             ▼         │             ▼
     ┌─────────┐    │        ┌─────────┐    │        ┌─────────┐
     │ Result  │    │        │ Result  │    │        │ Result  │
     │ Partial │    │        │ Partial │    │        │ Partial │
     └─────────┘    │        └─────────┘    │        └─────────┘
          │         │             │         │             │
          └─────────┼─────────────┼─────────┘             │
                    └─────────────┼───────────────────────┘
                                  ▼
                         ┌─────────────────┐
                         │ Result Assembly │
                         │ & Aggregation   │
                         └─────────┬───────┘
                                   ▼
                           ┌─────────────────┐
                           │ Output Writing  │
                           └─────────────────┘
```

## Performance Optimizations

### Multi-Threading Strategy

#### 1. Concurrency Limiting
```csharp
// Prevent thread pool exhaustion
private readonly SemaphoreSlim _concurrencyLimiter;
_maxConcurrency = Math.Min(Environment.ProcessorCount * 2, 16);
```

#### 2. Smart Parallelization
```csharp
// Only parallelize when beneficial
if (node.ChildNodes().Count() > 50)
{
    // Use parallel processing
    children = node.ChildNodes().AsParallel().AsOrdered()...
}
else
{
    // Use sequential processing
    children = node.ChildNodes().Select(...)...
}
```

#### 3. Memory Optimization
```csharp
// Object pooling reduces GC pressure
private readonly ArrayPool<ASTNode> _nodePool = ArrayPool<ASTNode>.Shared;
private readonly ConcurrentQueue<List<ASTNode>> _listPool = new();

// Reuse expensive objects
var pooledList = GetPooledList();
try 
{
    // Use list
}
finally 
{
    ReturnPooledList(pooledList);
}
```

### Performance Characteristics

| Feature | Standard | Optimized | Improvement |
|---------|----------|-----------|-------------|
| Memory Allocation | High GC pressure | Pooled objects | 60% reduction |
| CPU Utilization | Single-threaded | Multi-threaded | 4-24x speedup |
| I/O Efficiency | Sequential | Concurrent | 3-5x throughput |
| Large Files | Slower | Parallel analysis | 2-3x speedup |

## Extensibility Points

### 1. Custom Analyzers

Implement `ISyntaxAnalyzer` for domain-specific analysis:

```csharp
public class SecurityPatternAnalyzer : ISyntaxAnalyzer
{
    public ASTAnalysis AnalyzeSyntaxTree(SyntaxNode root, string filePath)
    {
        // Custom security pattern detection
        return new ASTAnalysis
        {
            // Add security-specific analysis
            Properties = DetectSecurityPatterns(root)
        };
    }
}
```

### 2. Custom Output Formats

Extend `IOutputManager` for new output formats:

```csharp
public class GraphQLOutputManager : IOutputManager
{
    public async Task WriteAsync(ASTAnalysis analysis, string outputPath, string format)
    {
        if (format == "graphql")
        {
            var schema = GenerateGraphQLSchema(analysis);
            await File.WriteAllTextAsync(outputPath, schema);
        }
    }
}
```

### 3. Custom File Processors

Implement `IFileProcessor` for specialized processing:

```csharp
public class DatabaseSchemaProcessor : IFileProcessor
{
    public async Task<ASTAnalysis?> ProcessFileAsync(string filePath)
    {
        // Process .sql files or other database schemas
        // Generate AST-like structure for database objects
    }
}
```

## Error Handling Strategy

### 1. Layered Error Handling

```
Application Layer
│ ┌─────────────────────────────────────────┐
│ │ • User-friendly error messages         │
│ │ • Graceful degradation                 │
│ │ • Partial result recovery              │
│ └─────────────────────────────────────────┘
│
Domain Layer
│ ┌─────────────────────────────────────────┐
│ │ • Business logic validation            │
│ │ • Domain-specific error handling       │
│ │ • Error aggregation and reporting      │
│ └─────────────────────────────────────────┘
│
Infrastructure Layer
│ ┌─────────────────────────────────────────┐
│ │ • File I/O error handling              │
│ │ • Network timeout handling             │
│ │ • Resource cleanup                     │
│ └─────────────────────────────────────────┘
```

### 2. Error Recovery Patterns

#### File-Level Errors
```csharp
try
{
    var fileAnalysis = await ProcessCSharpFileAsync(file);
    results.Add(fileAnalysis);
}
catch (Exception ex)
{
    // Log error but continue processing other files
    _logger.LogError(ex, "Failed to process file: {FilePath}", file);
    results.Add(CreateErrorAnalysis(file, ex));
}
```

#### Project-Level Errors
```csharp
var partialResults = new List<ASTAnalysis>();
var errors = new List<ProcessingError>();

foreach (var file in projectFiles)
{
    try
    {
        var result = await ProcessFileAsync(file);
        partialResults.Add(result);
    }
    catch (Exception ex)
    {
        errors.Add(new ProcessingError(file, ex));
        // Continue with other files
    }
}

return new ProjectAnalysis
{
    Results = partialResults,
    Errors = errors,
    IsPartial = errors.Any()
};
```

## Testing Strategy

### 1. Unit Testing

```csharp
[Test]
public async Task SyntaxAnalyzer_AnalyzesAsyncMethod_ReturnsCorrectMetadata()
{
    // Arrange
    var sourceCode = @"
        public async Task<string> GetDataAsync()
        {
            return await _service.FetchAsync();
        }";
    
    var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
    var analyzer = new SyntaxAnalyzer();
    
    // Act
    var result = analyzer.AnalyzeSyntaxTree(syntaxTree.GetRoot(), "test.cs");
    
    // Assert
    Assert.That(result.AsyncMethods, Has.Count.EqualTo(1));
    Assert.That(result.AsyncMethods[0].Name, Is.EqualTo("GetDataAsync"));
}
```

### 2. Integration Testing

```csharp
[Test]
public async Task ASTGenerator_ProcessesProject_GeneratesCompleteAnalysis()
{
    // Arrange
    var projectPath = "TestProjects/SampleProject.csproj";
    var generator = new ASTGenerator();
    
    // Act
    var analysis = await generator.ProcessProjectAsync(projectPath);
    
    // Assert
    Assert.That(analysis, Is.Not.Null);
    Assert.That(analysis.Files, Has.Count.GreaterThan(0));
    Assert.That(analysis.Dependencies, Is.Not.Empty);
}
```

### 3. Performance Testing

```csharp
[Test]
public async Task OptimizedGenerator_OutperformsStandard_InLargeProjects()
{
    // Arrange
    var largeProjectPath = "TestProjects/LargeProject.csproj";
    var standardGenerator = ASTGenerator.Create();
    var optimizedGenerator = ASTGenerator.CreateOptimized();
    
    // Act
    var standardTime = await MeasureExecutionTime(() => 
        standardGenerator.ProcessProjectAsync(largeProjectPath));
    var optimizedTime = await MeasureExecutionTime(() => 
        optimizedGenerator.ProcessProjectAsync(largeProjectPath));
    
    // Assert
    var speedup = standardTime / optimizedTime;
    Assert.That(speedup, Is.GreaterThan(2.0), "Optimized version should be at least 2x faster");
}
```

## Threading Model

### 1. Task-Based Asynchrony

All I/O operations use `async/await` for non-blocking execution:

```csharp
public async Task<ASTAnalysis?> ProcessCSharpFileAsync(string filePath, CancellationToken cancellationToken = default)
{
    // Non-blocking file read
    var sourceText = await File.ReadAllTextAsync(filePath, cancellationToken);
    
    // CPU-bound parsing
    var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
    var root = await syntaxTree.GetRootAsync(cancellationToken);
    
    // Analysis with potential parallel processing
    return _syntaxAnalyzer.AnalyzeSyntaxTree(root, filePath);
}
```

### 2. Parallel Processing

CPU-intensive operations use `Parallel.ForEach` and `PLINQ`:

```csharp
// File-level parallelism
await Parallel.ForEachAsync(files, new ParallelOptions
{
    MaxDegreeOfParallelism = _maxConcurrency,
    CancellationToken = cancellationToken
}, async (file, ct) =>
{
    var analysis = await ProcessFileAsync(file, ct);
    results.Add(analysis);
});

// Node-level parallelism
var children = largeNodeList
    .AsParallel()
    .AsOrdered()
    .Select(child => AnalyzeNode(child))
    .ToList();
```

### 3. Concurrency Control

```csharp
// Semaphore prevents resource exhaustion
private readonly SemaphoreSlim _concurrencyLimiter;

await _concurrencyLimiter.WaitAsync(cancellationToken);
try
{
    // Protected resource access
    return await ProcessFileAsync(filePath);
}
finally
{
    _concurrencyLimiter.Release();
}
```

## Design Patterns

### 1. Strategy Pattern
- Multiple implementations of `ISyntaxAnalyzer`
- Swappable analysis strategies at runtime

### 2. Dependency Injection
- Constructor injection for all dependencies
- Interface-based loose coupling

### 3. Object Pool Pattern
- Memory optimization through object reuse
- Reduced GC pressure in high-throughput scenarios

### 4. Template Method Pattern
- Base processing flow with customizable steps
- Consistent error handling and logging

### 5. Observer Pattern
- Progress reporting and event notification
- Extensible logging and monitoring

---

This design document provides the technical foundation for understanding and extending the CSharpAST system. For implementation details and specifications, see [SPECS.md](SPECS.md).
