# C# AST Generator

A comprehensive command-line tool and library for generating Abstract Syntax Trees (AST) from C# source code, with specialized features for analyzing async/await patterns, generating test scaffolding, and providing detailed code structure analysis.

## Overview

The C# AST Generator is a powerful tool built on top of Microsoft's Roslyn compiler APIs that provides deep analysis of C# codebases. It can process individual files, entire projects, or complete solutions, generating detailed JSON, XML, or text reports that capture the syntactic structure and semantic patterns in your code.

### Key Capabilities

- **🔍 Deep AST Analysis**: Complete syntax tree generation with node-level details
- **⚡ High-Performance Processing**: Multi-threaded analysis with up to 24x performance improvements
- **🧪 Test Generation**: Automated mock class and test fixture creation
- **📊 Async Pattern Analysis**: Specialized detection and analysis of async/await patterns
- **🏗️ Project-Level Insights**: Cross-file dependency analysis and architectural insights
- **📈 Performance Metrics**: Built-in benchmarking and performance analysis tools

## Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- Windows, macOS, or Linux

### Installation & Build

```bash
# Clone the repository
git clone https://github.com/your-username/CSharpAST.git
cd CSharpAST

# Build the solution
dotnet build

# Run tests to verify installation
dotnet test
```

### Basic Usage

```bash
# Analyze a single C# file
dotnet run --project CSharpAST.CLI -- --input "src/MyClass.cs" --output "analysis" --format json

# Analyze an entire project (recommended)
dotnet run --project CSharpAST.CLI -- --input "MyProject.csproj" --output "output" --format json

# Analyze a complete solution
dotnet run --project CSharpAST.CLI -- --input "MySolution.sln" --output "solution-analysis" --format json

# Performance testing with optimized processing
dotnet run --project CSharpAST.PerformanceTest
```

## Command Line Interface

### Arguments

| Argument | Short | Description | Required | Default |
|----------|-------|-------------|----------|---------|
| `--input` | `-i` | Path to C# file, project (.csproj), solution (.sln), or directory | ✅ | - |
| `--output` | `-o` | Output directory for generated analysis files | ✅ | - |
| `--format` | `-f` | Output format: `json`, `xml`, or `text` | ❌ | `json` |
| `--verbose` | `-v` | Enable detailed logging and progress output | ❌ | `false` |

### Examples

```bash
# Generate detailed JSON analysis with verbose output
dotnet run --project CSharpAST.CLI -- -i "src/" -o "reports" -f json -v

# Quick text summary of async patterns
dotnet run --project CSharpAST.CLI -- -i "AsyncService.cs" -o "analysis" -f text

# XML output for integration with external tools
dotnet run --project CSharpAST.CLI -- -i "MyProject.csproj" -o "xml-reports" -f xml
```

## Project Structure

```
CSharpAST/
├── CSharpAST.Core/              # Core AST generation and analysis engine
│   ├── Analysis/                # Syntax analyzers and AST processors
│   ├── Processing/              # File and project processors
│   ├── Output/                  # Output formatters and managers
│   └── Models.cs                # Data models and DTOs
├── CSharpAST.CLI/               # Command-line interface
├── CSharpAST.TestGeneration/    # Test scaffolding and mock generation
├── CSharpAST.Performance/       # Performance benchmarking tools
├── CSharpAST.PerformanceTest/   # Performance testing application
├── CSharpAST.IntegrationTests/  # Integration test suite
├── TestFiles/                   # Sample files for testing
└── docs/                        # Documentation (DESIGN.md, SPECS.md)
```

## Performance Optimizations

The tool includes a high-performance `OptimizedASTGenerator` with multi-threading capabilities:

- **4.65x overall speedup** on average across all scenarios
- **24x faster project-level processing** compared to sequential analysis
- **Intelligent concurrency scaling** (optimal at 4-8 threads)
- **Memory-optimized processing** with object pooling and ArrayPool usage

### Performance Comparison

| Scenario | Standard | Optimized | Speedup |
|----------|----------|-----------|---------|
| Single File (4KB) | 17.3ms | 8.3ms | 2.08x |
| Multiple Files (12 files) | 116.7ms | 49ms | 3.35x |
| Project Analysis | 113.7ms | 4.7ms | **24.36x** |

## Features

## Features

### Core Analysis Capabilities

- **📋 Comprehensive AST Generation**: Complete abstract syntax tree with full node hierarchy
- **🔄 Async Pattern Detection**: Deep analysis of async/await patterns, Task usage, and ConfigureAwait
- **🧩 Dependency Mapping**: Cross-file and cross-project dependency analysis
- **🏷️ Rich Metadata Extraction**: Classes, interfaces, methods, properties, fields, and attributes
- **📦 Project Structure Analysis**: Solution-wide architectural insights and patterns

### Advanced Features

- **⚡ Multi-Threaded Processing**: Concurrent file analysis with intelligent load balancing
- **🧪 Automated Test Generation**: Mock classes, test fixtures, and async test patterns
- **📊 Performance Benchmarking**: Built-in performance analysis and optimization recommendations
- **🔍 Pattern Recognition**: Detection of design patterns, code smells, and best practices
- **📈 Scalable Architecture**: Component-based design with dependency injection

### Output Formats

- **JSON**: Structured data perfect for programmatic consumption and integration
- **XML**: Enterprise-friendly format for tool integration and reporting
- **Text**: Human-readable summaries and reports for quick analysis

## Installation

### From Source

```bash
git clone https://github.com/your-username/CSharpAST.git
cd CSharpAST
dotnet restore
dotnet build --configuration Release
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run performance benchmarks with thread scaling analysis
dotnet run --project CSharpAST.PerformanceTest

# Run integration tests
dotnet test CSharpAST.IntegrationTests
```

## Performance Testing

The `CSharpAST.PerformanceTest` application provides comprehensive performance analysis including:

- **Standard vs Optimized Benchmarks**: Compare single-threaded vs multi-threaded performance
- **Thread Scaling Analysis**: Test performance across different thread counts (1-32 threads)
- **Memory Usage Monitoring**: Track memory consumption during processing
- **Concurrency Benefits**: Analyze optimal thread counts for your hardware

Example output:
```
Thread Count | Time (ms) | Speedup | Memory (MB)
-------------|-----------|---------|------------
           1 |     133.0 |    1.00x |        6.9
           4 |      61.3 |    2.17x |        7.9
          16 |      61.3 |    2.17x |        8.4
```

## Usage

### Command Line Interface

#### Basic Commands

```bash
# Analyze a single C# file
dotnet run --project CSharpAST.CLI -- --input "path/to/file.cs" --output "output/directory"

# Analyze an entire directory
dotnet run --project CSharpAST.CLI -- --input "src/directory" --output "ast_output"

# Analyze a C# project (recommended approach)
dotnet run --project CSharpAST.CLI -- --input "MyProject.csproj" --output "project_analysis"

# Analyze an entire solution
dotnet run --project CSharpAST.CLI -- --input "MySolution.sln" --output "solution_analysis"
```

#### Advanced Usage

```bash
# High-performance analysis with verbose output
dotnet run --project CSharpAST.CLI -- -i "MyProject.csproj" -o "analysis" -f json -v

# Generate XML reports for CI/CD integration
dotnet run --project CSharpAST.CLI -- -i "src/" -o "reports" -f xml

# Quick text summary for code review
dotnet run --project CSharpAST.CLI -- -i "MyClass.cs" -o "summary" -f text
```

## Output Structure

The generated AST includes:

### File-Level Analysis (JSON Output)
- **Full AST Tree**: Complete syntax tree with node types, spans, and properties
- **Async Methods**: Detailed analysis of async methods including await expressions
- **Classes/Interfaces**: Information about type declarations, inheritance, and members
- **Properties**: Auto-implemented properties, getters/setters analysis
- **Using Directives**: Namespace imports and dependencies

### Project-Level Analysis
- **Project Metadata**: Dependencies, file count, project structure
- **Async Patterns**: Cross-file async pattern analysis with ConfigureAwait, Task.WhenAll detection
- **Test Class Detection**: Automatic identification of test classes and methods
- **Generated Test Data**: Mock classes, test fixtures, and async test patterns

### Solution-Level Analysis
- **Multi-Project Overview**: Analysis across all projects in a solution
- **Dependency Mapping**: Cross-project dependencies and references
- **Integration Test Suggestions**: Recommended integration test scenarios

### Special Focus on Async Patterns

The tool provides detailed analysis of:
- `async` method declarations
- `await` expressions and their targets
- `Task`, `Task<T>`, and `ValueTask` return types
- `IAsyncEnumerable` patterns
- Async lambda expressions
- ConfigureAwait usage

## Sample Output

For an async method like:
```csharp
public async Task<string> GetDataAsync(string url)
{
    var response = await _httpClient.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}
```

The tool generates:
```json
{
  "asyncMethods": [
    {
      "name": "GetDataAsync",
      "returnType": "Task<string>",
      "parameters": ["string url"],
      "awaitExpressions": [
        "_httpClient.GetAsync(url)",
        "response.Content.ReadAsStringAsync()"
      ],
      "lineNumber": 15
    }
  ]
}
```

## Test Data Generation

The tool automatically generates comprehensive test data structures suitable for unit and integration testing:

### Mock Classes
For each interface found in the code, generates mock class definitions:
```json
{
  "mockClasses": [
    {
      "interfaceName": "IDataProcessor",
      "mockClassName": "MockDataProcessor",
      "methods": [
        {
          "methodName": "ProcessAsync",
          "returnType": "Task<string>",
          "isAsync": true
        }
      ]
    }
  ]
}
```

### Test Fixtures
Generates test class scaffolding for each non-test class:
```json
{
  "testFixtures": [
    {
      "className": "DataProcessor",
      "testClassName": "DataProcessorTests",
      "testMethods": [
        {
          "methodName": "Test_ProcessAsync",
          "testedMethod": "ProcessAsync",
          "testType": "Unit"
        }
      ]
    }
  ]
}
```

### Async Test Patterns
Specialized test patterns for async methods with analysis of await patterns:
```json
{
  "asyncTestPatterns": [
    {
      "methodName": "ProcessAsync",
      "testName": "Test_ProcessAsync_Async",
      "expectedAwaitCount": 3,
      "shouldTestConfigureAwait": true,
      "shouldTestCancellation": true,
      "suggestedAssertions": [
        "Assert.IsNotNull(result)",
        "Assert.ConfigureAwait(false) used appropriately"
      ]
    }
  ]
}
```

## Documentation

- **[Design Document](docs/DESIGN.md)**: Detailed architectural design and component interactions
- **[Specifications](docs/SPECS.md)**: Complete technical specifications for LLM consumption

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

*Built with ❤️ using .NET 8, Roslyn, and modern C# practices*
