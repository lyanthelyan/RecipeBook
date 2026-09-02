using CommonTestUtilities.Entities;
using CommonTestUtilities.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyRecipeBook.Domain.Security.PasswordHashing;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.Storage;
using MyRecipeBook.Infrastructure.DataAcess;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
using WebApi.Tests.Resources;

namespace WebApi.Tests;

public class MyRecipeBookApplicationFactory: WebApplicationFactory<Program>, IAsyncLifetime
{
    public UserIdentityManager User1 { get; private set; } = default!;
    public string TokenUserNotFoundInDatabase { get; private set; } = string.Empty;
    private MyRecipeBook.Domain.Entities.User _user = default!;
    private MyRecipeBook.Domain.Entities.Recipe _recipe = default!;
    private readonly MsSqlContainer _msSqlContainer;
    private readonly AzuriteContainer _azuriteContainer;
    
    public MyRecipeBookApplicationFactory()
    {
        _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU10-ubuntu-22.04")
            .Build();
        _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.23.0")
        .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Tests");

        builder.ConfigureAppConfiguration((context, configuration) =>
        {

            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _msSqlContainer
                        .GetConnectionString(),
                    ["ConnectionStrings:BlobStorage"] = _azuriteContainer
                        .GetConnectionString()
                });
        });
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        await _azuriteContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        
        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher>();
        var accessTokenGenerator = scope.ServiceProvider
            .GetRequiredService<IAccessTokensGenerator>();

        var (user, password) = UserBuilder.Build();
        user.Password = passwordHasher.HashPassword(password);

        _user = user;
        _recipe = RecipeBuilder.Build(user);

        var user1AccessToken = accessTokenGenerator.Generate(user);

        User1 = new UserIdentityManager(user, _recipe, password, user1AccessToken);

        var (userNotSaved, _) = UserBuilder.Build();
        TokenUserNotFoundInDatabase = accessTokenGenerator.Generate(userNotSaved);
        
    }

    public async Task ResetDatabase()
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<MyRecipeBookDbContext>();

        await dbContext.VerificationCodes.ExecuteDeleteAsync();
        await dbContext.Recipes.ExecuteDeleteAsync();
        await dbContext.Users.ExecuteDeleteAsync();

        await dbContext.Users.AddAsync(_user);
        await dbContext.Recipes.AddAsync(_recipe);

        await dbContext.SaveChangesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
        await _azuriteContainer.DisposeAsync();
        
    }
}
