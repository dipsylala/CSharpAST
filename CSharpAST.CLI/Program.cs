using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using CSharpAST.Core;
using CSharpAST.Core.Analysis;
using CSharpAST.Core.Processing;
using CSharpAST.Core.OutputManager;

namespace CSharpAST.CLI;

/// <summary>
/// Command-line interface for CSharpAST with support for solutions, projects, and individual files
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("CSharpAST - Multi-language AST analyzer for .NET projects")
        {
            CreateInputOption(),
            CreateOutputOption(),
            CreateFormatOption(),
            CreateProcessorOption(),
            CreateVerboseOption(),
            CreateConcurrencyOption()
        };

        rootCommand.SetHandler(async (inputs, output, format, processor, verbose, concurrency) =>
        {
            await ProcessInputsAsync(inputs, output, format, processor, verbose, concurrency);
        },
        new InputsBinder(),
        new OutputBinder(),
        new FormatBinder(),
        new ProcessorBinder(),
        new VerboseBinder(),
        new ConcurrencyBinder());

        return await rootCommand.InvokeAsync(args);
    }

    private static Option<List<string>> CreateInputOption()
    {
        var option = new Option<List<string>>(
            aliases: new[] { "--input", "-i" },
            description: "Input files or paths. Supports:\n" +
                        "  • Solution files (.sln)\n" +
                        "  • Project files (.csproj, .vbproj)\n" +
                        "  • Source files (.cs, .vb, .cshtml)\n" +
                        "  • Multiple files can be specified")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };

        option.AddValidator(result =>
        {
            var inputs = result.GetValueForOption(option);
            if (inputs == null || inputs.Count == 0)
            {
                result.ErrorMessage = "At least one input file must be specified";
                return;
            }

            foreach (var input in inputs)
            {
                if (!File.Exists(input))
                {
                    result.ErrorMessage = $"Input file does not exist: {input}";
                    return;
                }

                var extension = Path.GetExtension(input).ToLowerInvariant();
                var supportedExtensions = new[] { ".sln", ".csproj", ".vbproj", ".cs", ".vb", ".cshtml" };
                
                if (!supportedExtensions.Contains(extension))
                {
                    result.ErrorMessage = $"Unsupported file type: {input}. Supported types: {string.Join(", ", supportedExtensions)}";
                    return;
                }
            }
        });

        return option;
    }

    private static Option<string> CreateOutputOption()
    {
        return new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Output directory path",
            getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "Output"));
    }

    private static Option<string> CreateFormatOption()
    {
        var option = new Option<string>(
            aliases: new[] { "--format", "-f" },
            description: "Output format",
            getDefaultValue: () => "json");

        option.FromAmong("json", "xml", "yaml", "text");
        return option;
    }

    private static Option<string> CreateProcessorOption()
    {
        var option = new Option<string>(
            aliases: new[] { "--processor", "-p" },
            description: "Processing mode:\n" +
                        "  • unified: Sequential multi-language processing\n" +
                        "  • concurrent: Parallel multi-language processing",
            getDefaultValue: () => "concurrent");

        option.FromAmong("unified", "concurrent");
        return option;
    }

    private static Option<bool> CreateVerboseOption()
    {
        return new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Enable verbose output");
    }

    private static Option<int> CreateConcurrencyOption()
    {
        return new Option<int>(
            aliases: new[] { "--concurrency", "-c" },
            description: "Maximum concurrency level (default: auto-detect)",
            getDefaultValue: () => Math.Min(Environment.ProcessorCount * 2, 16));
    }

    /// <summary>
    /// Creates the appropriate output manager based on the format
    /// </summary>
    private static IOutputManager CreateOutputManager(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "json" => new JsonOutputManager(),
            "text" or "txt" => new TextOutputManager(),
            _ => throw new ArgumentException($"Unsupported output format: {format}")
        };
    }

    private static async Task ProcessInputsAsync(List<string> inputs, string outputPath, string format, 
        string processor, bool verbose, int concurrency)
    {
        try
        {
            // Create appropriate output manager based on format
            var outputManager = CreateOutputManager(format);
            
            // Create appropriate AST generator based on processor type
            var processingMode = processor switch
            {
                "unified" => ASTGenerator.ProcessingMode.Unified,
                "concurrent" => ASTGenerator.ProcessingMode.Concurrent,
                _ => throw new ArgumentException($"Unknown processor type: {processor}")
            };

            var generator = new ASTGenerator(outputManager, processingMode, verbose, concurrency);

            if (verbose)
            {
                Console.WriteLine($"Using {processor} processor with concurrency level: {concurrency}");
                Console.WriteLine($"Processing {inputs.Count} input(s):");
                foreach (var input in inputs)
                {
                    Console.WriteLine($"  • {input} ({GetInputType(input)})");
                }
                Console.WriteLine();
            }

            // Group inputs by type for efficient processing
            var inputGroups = inputs.GroupBy(GetInputType).ToList();

            foreach (var group in inputGroups)
            {
                switch (group.Key)
                {
                    case InputType.Solution:
                        await ProcessSolutionFilesAsync(generator, group.ToList(), outputPath, format, verbose);
                        break;
                    case InputType.Project:
                        await ProcessProjectFilesAsync(generator, group.ToList(), outputPath, format, verbose);
                        break;
                    case InputType.SourceFile:
                        await ProcessSourceFilesAsync(generator, group.ToList(), outputPath, format, verbose);
                        break;
                }
            }

            Console.WriteLine("✅ Processing completed successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            if (verbose)
            {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            Environment.Exit(1);
        }
    }

    private static async Task ProcessSolutionFilesAsync(ASTGenerator generator, List<string> solutionFiles, 
        string outputPath, string format, bool verbose)
    {
        foreach (var solutionFile in solutionFiles)
        {
            if (verbose)
                Console.WriteLine($"Processing solution: {Path.GetFileName(solutionFile)}");

            var outputDir = Path.Combine(outputPath, "Solutions", Path.GetFileNameWithoutExtension(solutionFile));
            await generator.GenerateASTAsync(solutionFile, outputDir);
        }
    }

    private static async Task ProcessProjectFilesAsync(ASTGenerator generator, List<string> projectFiles, 
        string outputPath, string format, bool verbose)
    {
        foreach (var projectFile in projectFiles)
        {
            if (verbose)
                Console.WriteLine($"Processing project: {Path.GetFileName(projectFile)}");

            var outputDir = Path.Combine(outputPath, "Projects", Path.GetFileNameWithoutExtension(projectFile));
            await generator.GenerateASTAsync(projectFile, outputDir);
        }
    }

    private static async Task ProcessSourceFilesAsync(ASTGenerator generator, List<string> sourceFiles, 
        string outputPath, string format, bool verbose)
    {
        if (sourceFiles.Count == 1)
        {
            // Single file processing
            var sourceFile = sourceFiles[0];
            if (verbose)
                Console.WriteLine($"Processing source file: {Path.GetFileName(sourceFile)}");

            var outputDir = Path.Combine(outputPath, "SourceFiles", Path.GetFileNameWithoutExtension(sourceFile));
            await generator.GenerateASTAsync(sourceFile, outputDir);
        }
        else
        {
            // Multiple source files - create a combined analysis
            if (verbose)
                Console.WriteLine($"Processing {sourceFiles.Count} source files as batch");

            var batchOutputDir = Path.Combine(outputPath, "SourceFiles", "BatchAnalysis");
            Directory.CreateDirectory(batchOutputDir);

            // Process each file individually and create a combined report
            var analyses = new List<(string FilePath, ASTAnalysis? Analysis)>();
            
            foreach (var sourceFile in sourceFiles)
            {
                try
                {
                    var analysis = await generator.ProcessFileAsync(sourceFile);
                    analyses.Add((sourceFile, analysis));
                    
                    if (verbose)
                        Console.WriteLine($"  ✓ {Path.GetFileName(sourceFile)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ {Path.GetFileName(sourceFile)}: {ex.Message}");
                    analyses.Add((sourceFile, null));
                }
            }

            // Create combined analysis
            var combinedAnalysis = new ASTAnalysis
            {
                SourceFile = "BatchAnalysis",
                GeneratedAt = DateTime.UtcNow,
                RootNode = new ASTNode
                {
                    Type = "BatchRoot",
                    Kind = "Batch",
                    Text = $"Batch Analysis of {sourceFiles.Count} files",
                    Properties = new Dictionary<string, object>
                    {
                        ["FileCount"] = sourceFiles.Count,
                        ["SuccessfulCount"] = analyses.Count(a => a.Analysis != null),
                        ["FailedCount"] = analyses.Count(a => a.Analysis == null)
                    },
                    Children = new List<ASTNode>()
                }
            };

            foreach (var (filePath, analysis) in analyses)
            {
                if (analysis?.RootNode != null)
                {
                    combinedAnalysis.RootNode.Children.Add(analysis.RootNode);
                }
                else
                {
                    combinedAnalysis.RootNode.Children.Add(new ASTNode
                    {
                        Type = "ErrorNode",
                        Kind = "Error",
                        Text = $"Failed to process: {Path.GetFileName(filePath)}",
                        Properties = new Dictionary<string, object> { ["FilePath"] = filePath },
                        Children = new List<ASTNode>()
                    });
                }
            }

            var batchOutputFile = Path.Combine(batchOutputDir, "BatchAnalysis.json");
            await generator.WriteOutputAsync(combinedAnalysis, batchOutputFile);
        }
    }

    private static InputType GetInputType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".sln" => InputType.Solution,
            ".csproj" or ".vbproj" => InputType.Project,
            ".cs" or ".vb" or ".cshtml" => InputType.SourceFile,
            _ => throw new ArgumentException($"Unsupported file type: {extension}")
        };
    }

    private enum InputType
    {
        Solution,
        Project,
        SourceFile
    }

    // Command line binders
    private class InputsBinder : BinderBase<List<string>>
    {
        protected override List<string> GetBoundValue(BindingContext bindingContext)
            => bindingContext.ParseResult.GetValueForOption(bindingContext.ParseResult.CommandResult.Command.Options.OfType<Option<List<string>>>().First()) ?? new List<string>();
    }

    private class OutputBinder : BinderBase<string>
    {
        protected override string GetBoundValue(BindingContext bindingContext)
            => bindingContext.ParseResult.GetValueForOption(bindingContext.ParseResult.CommandResult.Command.Options.OfType<Option<string>>().First(o => o.HasAlias("--output"))) ?? "Output";
    }

    private class FormatBinder : BinderBase<string>
    {
        protected override string GetBoundValue(BindingContext bindingContext)
            => bindingContext.ParseResult.GetValueForOption(bindingContext.ParseResult.CommandResult.Command.Options.OfType<Option<string>>().First(o => o.HasAlias("--format"))) ?? "json";
    }

    private class ProcessorBinder : BinderBase<string>
    {
        protected override string GetBoundValue(BindingContext bindingContext)
            => bindingContext.ParseResult.GetValueForOption(bindingContext.ParseResult.CommandResult.Command.Options.OfType<Option<string>>().First(o => o.HasAlias("--processor"))) ?? "concurrent";
    }

    private class VerboseBinder : BinderBase<bool>
    {
        protected override bool GetBoundValue(BindingContext bindingContext)
            => bindingContext.ParseResult.GetValueForOption(bindingContext.ParseResult.CommandResult.Command.Options.OfType<Option<bool>>().First());
    }

    private class ConcurrencyBinder : BinderBase<int>
    {
        protected override int GetBoundValue(BindingContext bindingContext)
            => bindingContext.ParseResult.GetValueForOption(bindingContext.ParseResult.CommandResult.Command.Options.OfType<Option<int>>().First());
    }
}
