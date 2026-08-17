using CommonTestUtilities.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Security.PasswordHashing;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Infrastructure.DataAcess;
using Testcontainers.MsSql;
using WebApi.Tests.Resources;

namespace WebApi.Tests;

public class MyRecipeBookApplicationFactory: WebApplicationFactory<Program>, IAsyncLifetime
{
    public UserIdentityManager User1 { get; private set; } = default!;
    public string TokenUserNotFoundInDatabase { get; private set; } = string.Empty;

    private string? _connectionString;
    
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU10-ubuntu-22.04").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Tests");

        builder.ConfigureAppConfiguration((context, configuration) =>
        {

            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString
                });
        });

    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<MyRecipeBookDbContext>();
        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher>();
        var accessTokenGenerator = scope.ServiceProvider
            .GetRequiredService<IAccessTokensGenerator>();
        
        var (user, password) = UserBuilder.Build();
        user.Password = passwordHasher.HashPassword(password);

        var recipe = RecipeBuilder.Build(user);

        await dbContext.Users.AddAsync(user);
        await dbContext.Recipes.AddAsync(recipe);
        
        await dbContext.SaveChangesAsync();
       
        var user1AccessToken = accessTokenGenerator.Generate(user);
        
        User1 = new UserIdentityManager(user, recipe, password, user1AccessToken);

        var (userNotSaved, _) = UserBuilder.Build();
        TokenUserNotFoundInDatabase = accessTokenGenerator.Generate(userNotSaved);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }
}