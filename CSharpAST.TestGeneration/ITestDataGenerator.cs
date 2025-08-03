using CSharpAST.Core;

namespace CSharpAST.TestGeneration;

public interface ITestDataGenerator
{
    TestDataCollection GenerateTestData(ProjectAnalysis projectAnalysis);
    TestDataCollection GenerateTestData(SolutionAnalysis solutionAnalysis);
}
