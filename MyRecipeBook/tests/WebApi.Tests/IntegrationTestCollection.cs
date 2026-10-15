namespace WebApi.Tests;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<MyRecipeBookApplicationFactory>
{
}
