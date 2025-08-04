using FluentAssertions;
using CSharpAST.Core.Analysis;
using Xunit;

namespace CSharpAST.IntegrationTests;

[Collection("CoreFunctionalityTests")]
public class VBAnalyzerDebugTests
{
    [Fact]
    public void VBAnalyzer_ShouldSupportVBFiles()
    {
        // Arrange
        var analyzer = new VBSyntaxAnalyzer();
        var vbFilePath = "test.vb";

        // Act
        var supportsFile = analyzer.Capabilities.SupportsFile(vbFilePath);

        // Assert
        supportsFile.Should().BeTrue("VB Analyzer should support .vb files");
    }

    [Fact]
    public void AnalyzerRegistry_ShouldReturnVBAnalyzerForVBFiles()
    {
        // Arrange
        var vbFilePath = Path.Combine("TestFiles", "SingleFiles", "VB", "BookModels.vb");

        // Act
        var analyzer = AnalyzerRegistry.GetAnalyzerForFile(vbFilePath);

        // Assert
        analyzer.Should().NotBeNull();
        analyzer.Should().BeOfType<VBSyntaxAnalyzer>();
    }

    [Fact]
    public void VBAnalyzer_ShouldParseSimpleVBCode()
    {
        // Arrange
        var analyzer = new VBSyntaxAnalyzer();
        var simpleVBCode = @"
Imports System

Namespace TestNamespace
    Public Class TestClass
        Public Property Name As String
    End Class
End Namespace";

        // Act
        var result = analyzer.AnalyzeFile("test.vb", simpleVBCode);

        // Assert
        result.Should().NotBeNull();
        result.RootNode.Should().NotBeNull();
        result.RootNode.Type.Should().Be("CompilationUnitSyntax");
    }
}
