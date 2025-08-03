using FluentAssertions;
using CSharpAST.Core;
using CSharpAST.Core.Processing;
using CSharpAST.Core.Analysis;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace CSharpAST.IntegrationTests
{
    public class RazorCSHTMLIntegrationTests : TestBase
    {

        [Fact]
        public async Task ProcessRazorFile_ShouldGenerateValidAST()
        {
            // Arrange
            var processor = new UnifiedFileProcessor(new SyntaxAnalyzer());
            var razorFilePath = Path.Combine(_testFilesPath, "SingleFiles", "Razor", "RazorSample.cshtml");
            
            // Assert file exists
            File.Exists(razorFilePath).Should().BeTrue($"Test file not found: {razorFilePath}");
            processor.IsFileSupported(razorFilePath).Should().BeTrue("CSHTML files should be supported");

            // Act
            var analysis = await processor.ProcessFileAsync(razorFilePath);

            // Assert
            analysis.Should().NotBeNull("Analysis should not be null");
            analysis!.SourceFile.Should().Be(razorFilePath);
            analysis.RootNode.Should().NotBeNull("Root node should not be null");
            analysis.RootNode.Type.Should().Be("RazorDocument");
            analysis.RootNode.Kind.Should().Be("RazorFile");
            
            // Verify properties
            analysis.RootNode.Properties.Should().ContainKey("FileName");
            analysis.RootNode.Properties.Should().ContainKey("FileType");
            analysis.RootNode.Properties["FileType"].Should().Be("Razor/CSHTML");
        }

        [Fact]
        public async Task ProcessRazorFile_ShouldExtractCSharpCodeBlocks()
        {
            // Arrange
            var processor = new UnifiedFileProcessor(new SyntaxAnalyzer());
            var razorFilePath = Path.Combine(_testFilesPath, "SingleFiles", "Razor", "RazorSample.cshtml");

            // Act
            var analysis = await processor.ProcessFileAsync(razorFilePath);

            // Assert
            analysis.Should().NotBeNull();
            analysis!.RootNode.Children.Should().NotBeNull();
            
            var csharpCodeBlocks = analysis.RootNode.Children
                .Where(c => c.Type == "CSharpCodeBlock")
                .ToList();
                
            csharpCodeBlocks.Should().NotBeEmpty("Should find C# code blocks");
            
            // Verify code block properties
            var firstCodeBlock = csharpCodeBlocks.First();
            firstCodeBlock.Properties.Should().ContainKey("CSharpCode", "Code block should contain C# code");
            firstCodeBlock.Properties.Should().ContainKey("LineNumber", "Code block should have line number");
        }

        [Fact]
        public async Task ProcessRazorFile_ShouldExtractCSharpExpressions()
        {
            // Arrange
            var processor = new UnifiedFileProcessor(new SyntaxAnalyzer());
            var razorFilePath = Path.Combine(_testFilesPath, "SingleFiles", "Razor", "RazorSample.cshtml");

            // Act
            var analysis = await processor.ProcessFileAsync(razorFilePath);

            // Assert
            analysis.Should().NotBeNull();
            analysis!.RootNode.Children.Should().NotBeNull();
            
            var csharpExpressions = analysis.RootNode.Children
                .Where(c => c.Type == "CSharpExpression")
                .ToList();
                
            csharpExpressions.Should().NotBeEmpty("Should find C# expressions");
            
            // Verify expressions contain expected patterns
            var modelExpressions = csharpExpressions
                .Where(e => e.Properties.ContainsKey("Expression") && 
                           e.Properties["Expression"].ToString()!.Contains("Model"))
                .ToList();
                
            modelExpressions.Should().NotBeEmpty("Should find @Model expressions");
        }

        [Fact]
        public async Task UnifiedFileProcessor_ShouldHandleBothCSharpAndRazor()
        {
            // Arrange
            var processor = new UnifiedFileProcessor(new SyntaxAnalyzer());
            var csharpFilePath = Path.Combine(_testFilesPath, "SingleFiles", "CSharp", "AsyncSample.cs");
            var razorFilePath = Path.Combine(_testFilesPath, "SingleFiles", "Razor", "RazorSample.cshtml");

            // Act
            var csharpAnalysis = await processor.ProcessFileAsync(csharpFilePath);
            var razorAnalysis = await processor.ProcessFileAsync(razorFilePath);

            // Assert
            csharpAnalysis.Should().NotBeNull("C# analysis should not be null");
            razorAnalysis.Should().NotBeNull("Razor analysis should not be null");
            
            // Verify different root node types
            csharpAnalysis!.RootNode.Type.Should().Be("CompilationUnitSyntax");
            razorAnalysis!.RootNode.Type.Should().Be("RazorDocument");
            
            // Both should have valid structure
            csharpAnalysis.RootNode.Children.Should().NotBeEmpty("C# should have child nodes");
            razorAnalysis.RootNode.Children.Should().NotBeEmpty("Razor should have child nodes");
        }

        [Fact]
        public async Task ASTGenerator_CreateUnified_ShouldProcessRazorFiles()
        {
            // Arrange
            var generator = ASTGenerator.CreateUnified(verbose: false);
            var razorFilePath = Path.Combine(_testFilesPath, "SingleFiles", "Razor", "RazorSample.cshtml");
            var outputPath = CreateStructuredOutputPath("SingleFiles/Razor/RazorSample.cshtml", "ASTGenerator_CreateUnified_ShouldProcessRazorFiles");

            try
            {
                // Act
                await generator.GenerateASTAsync(razorFilePath, outputPath, "json");

                // Assert - check what files were actually created
                File.Exists(outputPath).Should().BeTrue("Output file should be created");
                
                var outputContent = await File.ReadAllTextAsync(outputPath);
                outputContent.Should().Contain("RazorDocument", "Output should contain Razor document structure");
                outputContent.Should().Contain("CSharpCodeBlock", "Output should contain C# code blocks");
            }
            finally
            {
                generator.Dispose();
                CleanupOutput(Path.GetDirectoryName(outputPath)!);
            }
        }
    }
}
