namespace CSharpAST.IntegrationTests;

/// <summary>
/// Collection definition for core functionality tests that access project files.
/// This prevents file contention issues when multiple core functionality tests try to parse the same files simultaneously.
/// </summary>
[CollectionDefinition("CoreFunctionalityTests", DisableParallelization = true)]
public class CoreFunctionalityTestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

/// <summary>
/// Collection definition for file and application processing tests that access project files.
/// This prevents file contention issues when multiple file processing tests try to parse the same files simultaneously.
/// </summary>
[CollectionDefinition("FileAndApplicationTests", DisableParallelization = true)]
public class FileAndApplicationTestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
