using CSharpAST.Core;

namespace CSharpAST.TestGeneration;

public class TestDataGenerator : ITestDataGenerator
{
    public TestDataCollection GenerateTestData(ProjectAnalysis projectAnalysis)
    {
        var testData = new TestDataCollection
        {
            GeneratedAt = DateTime.UtcNow,
            MockClasses = new List<MockClassDefinition>(),
            TestFixtures = new List<TestFixtureDefinition>(),
            AsyncTestPatterns = new List<AsyncTestPattern>()
        };

        // Generate mock classes for interfaces
        foreach (var file in projectAnalysis.Files)
        {
            foreach (var interfaceInfo in file.Interfaces)
            {
                var mockClass = new MockClassDefinition
                {
                    InterfaceName = interfaceInfo.Name,
                    MockClassName = $"Mock{interfaceInfo.Name.TrimStart('I')}",
                    Methods = interfaceInfo.Methods.Select(m => new MockMethodDefinition
                    {
                        MethodName = m,
                        ReturnType = "object", // Simplified - would need more analysis for actual types
                        IsAsync = false // Would need to analyze method signatures
                    }).ToList()
                };
                testData.MockClasses.Add(mockClass);
            }

            // Generate test fixtures for classes
            foreach (var classInfo in file.Classes.Where(c => !c.Name.EndsWith("Test") && !c.Name.EndsWith("Tests")))
            {
                var testFixture = new TestFixtureDefinition
                {
                    ClassName = classInfo.Name,
                    TestClassName = $"{classInfo.Name}Tests",
                    TestMethods = classInfo.Methods.Select(m => new TestMethodDefinition
                    {
                        MethodName = $"Test_{m}",
                        TestedMethod = m,
                        TestType = "Unit"
                    }).ToList()
                };
                testData.TestFixtures.Add(testFixture);
            }
        }

        // Generate async test patterns
        foreach (var asyncPattern in projectAnalysis.AsyncPatterns)
        {
            var testPattern = new AsyncTestPattern
            {
                MethodName = asyncPattern.MethodName,
                TestName = $"Test_{asyncPattern.MethodName}_Async",
                ExpectedAwaitCount = asyncPattern.AwaitCount,
                ShouldTestConfigureAwait = asyncPattern.HasConfigureAwait,
                ShouldTestCancellation = asyncPattern.ReturnType.Contains("CancellationToken"),
                SuggestedAssertions = GenerateAsyncAssertions(asyncPattern)
            };
            testData.AsyncTestPatterns.Add(testPattern);
        }

        return testData;
    }

    public TestDataCollection GenerateTestData(SolutionAnalysis solutionAnalysis)
    {
        var testData = new TestDataCollection
        {
            GeneratedAt = DateTime.UtcNow,
            MockClasses = new List<MockClassDefinition>(),
            TestFixtures = new List<TestFixtureDefinition>(),
            AsyncTestPatterns = new List<AsyncTestPattern>(),
            IntegrationTests = new List<IntegrationTestDefinition>()
        };

        // Aggregate test data from all projects
        foreach (var project in solutionAnalysis.Projects)
        {
            var projectTestData = GenerateTestData(project);
            testData.MockClasses.AddRange(projectTestData.MockClasses);
            testData.TestFixtures.AddRange(projectTestData.TestFixtures);
            testData.AsyncTestPatterns.AddRange(projectTestData.AsyncTestPatterns);
        }

        // Generate integration tests for cross-project dependencies
        foreach (var project in solutionAnalysis.Projects)
        {
            var integrationTest = new IntegrationTestDefinition
            {
                ProjectName = project.ProjectName,
                TestName = $"{project.ProjectName}_IntegrationTests",
                Dependencies = project.Dependencies,
                SuggestedScenarios = new List<string>
                {
                    "Test project initialization",
                    "Test dependency injection",
                    "Test async workflows"
                }
            };
            testData.IntegrationTests.Add(integrationTest);
        }

        return testData;
    }

    private List<string> GenerateAsyncAssertions(AsyncPatternInfo pattern)
    {
        var assertions = new List<string>();

        if (pattern.ReturnType.Contains("Task<"))
        {
            assertions.Add("Assert.IsNotNull(result)");
            assertions.Add("Assert.IsInstanceOfType(result, typeof(Task))");
        }

        if (pattern.HasTaskWhenAll)
        {
            assertions.Add("Assert.Multiple tasks completed successfully");
        }

        if (pattern.HasConfigureAwait)
        {
            assertions.Add("Assert.ConfigureAwait(false) used appropriately");
        }

        return assertions;
    }
}
