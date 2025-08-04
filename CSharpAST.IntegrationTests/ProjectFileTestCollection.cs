namespace CSharpAST.IntegrationTests;

/// <summary>
/// Collection definition for tests that access project files to prevent parallel execution.
/// This prevents file contention issues when multiple tests try to parse the same project files simultaneously.
/// </summary>
[CollectionDefinition("ProjectFileTests", DisableParallelization = true)]
public class ProjectFileTestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
