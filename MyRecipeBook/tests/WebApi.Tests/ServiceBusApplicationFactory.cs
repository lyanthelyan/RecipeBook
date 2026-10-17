namespace WebApi.Tests;

public class ServiceBusApplicationFactory : MyRecipeBookApplicationFactory
{
    public async override Task InitializeAsync()
    {
        await _serviceBusContainer.StartAsync();

        await base.InitializeAsync();
    }
}
