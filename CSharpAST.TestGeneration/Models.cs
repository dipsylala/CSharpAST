namespace CSharpAST.TestGeneration;

// Test Data Generation Models
public class TestDataCollection
{
    public DateTime GeneratedAt { get; set; }
    public List<MockClassDefinition> MockClasses { get; set; } = new List<MockClassDefinition>();
    public List<TestFixtureDefinition> TestFixtures { get; set; } = new List<TestFixtureDefinition>();
    public List<AsyncTestPattern> AsyncTestPatterns { get; set; } = new List<AsyncTestPattern>();
    public List<IntegrationTestDefinition> IntegrationTests { get; set; } = new List<IntegrationTestDefinition>();
}

public class MockClassDefinition
{
    public string InterfaceName { get; set; } = string.Empty;
    public string MockClassName { get; set; } = string.Empty;
    public List<MockMethodDefinition> Methods { get; set; } = new List<MockMethodDefinition>();
}

public class MockMethodDefinition
{
    public string MethodName { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public bool IsAsync { get; set; }
    public List<string> Parameters { get; set; } = new List<string>();
}

public class TestFixtureDefinition
{
    public string ClassName { get; set; } = string.Empty;
    public string TestClassName { get; set; } = string.Empty;
    public List<TestMethodDefinition> TestMethods { get; set; } = new List<TestMethodDefinition>();
}

public class TestMethodDefinition
{
    public string MethodName { get; set; } = string.Empty;
    public string TestedMethod { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty; // Unit, Integration, etc.
    public List<string> Assertions { get; set; } = new List<string>();
}

public class AsyncTestPattern
{
    public string MethodName { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public int ExpectedAwaitCount { get; set; }
    public bool ShouldTestConfigureAwait { get; set; }
    public bool ShouldTestCancellation { get; set; }
    public List<string> SuggestedAssertions { get; set; } = new List<string>();
}

public class IntegrationTestDefinition
{
    public string ProjectName { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new List<string>();
    public List<string> SuggestedScenarios { get; set; } = new List<string>();
}
