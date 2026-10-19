namespace WebApi.Tests;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<MyRecipeBookApplicationFactory>
{
}
[CollectionDefinition(nameof(ServiceBusIntegrationTestCollection), DisableParallelization = true)]
public class ServiceBusIntegrationTestCollection : ICollectionFixture<ServiceBusApplicationFactory>
{
}
