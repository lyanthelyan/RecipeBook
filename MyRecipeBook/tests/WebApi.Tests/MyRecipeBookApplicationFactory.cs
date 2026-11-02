using CommonTestUtilities.AI;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Storage;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MyRecipeBook.Api.BackgroundServices;
using MyRecipeBook.Domain.AI;
using MyRecipeBook.Domain.Security.PasswordHashing;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.Storage;
using MyRecipeBook.Infrastructure.DataAcess;
using System.Diagnostics;
using Testcontainers.MsSql;
using Testcontainers.ServiceBus;
using WebApi.Tests.Resources;

namespace WebApi.Tests;

public class MyRecipeBookApplicationFactory: WebApplicationFactory<Program>, IAsyncLifetime
{
    public UserIdentityManager User1 { get; private set; } = default!;
    public string TokenUserNotFoundInDatabase { get; private set; } = string.Empty;
    private MyRecipeBook.Domain.Entities.User _user = default!;
    private MyRecipeBook.Domain.Entities.Recipe _recipe = default!;
    private readonly MsSqlContainer _msSqlContainer;
    private readonly ServiceBusContainer? _serviceBusContainer;
    private readonly INetwork? _network;
    public Mock<IStorageService> StorageMock { get; } =
        Mock.Get(IStorageServiceBuilder.Build());

    public MyRecipeBookApplicationFactory()
        : this(false)
    {
    }

    protected MyRecipeBookApplicationFactory(bool useServiceBus)
    {
        var sqlBuilder = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU10-ubuntu-22.04")
            .WithDatabase("MyRecipeBookTests")
            .WithTmpfsMount("/var/opt/mssql/data")
            .WithPassword(MsSqlBuilder.DefaultPassword);

        if (useServiceBus)
        {
            _network = new NetworkBuilder().Build();
            sqlBuilder = sqlBuilder.WithNetwork(_network)
                .WithNetworkAliases(ServiceBusBuilder.DatabaseNetworkAlias);
        }

        _msSqlContainer = sqlBuilder.Build();

        if (useServiceBus)
        {
            _serviceBusContainer = new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
                .WithAcceptLicenseAgreement(true)
                .WithMsSqlContainer(_network!, _msSqlContainer,
                    ServiceBusBuilder.DatabaseNetworkAlias, MsSqlBuilder.DefaultPassword)
                .WithConfig(Path.Combine(AppContext.BaseDirectory, "ServiceBusConfig.json"))
                .Build();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Tests");

        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        _msSqlContainer.GetConnectionString()
                });

            if (_serviceBusContainer?.State == TestcontainersStates.Running)
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:ServiceBus"] =
                            _serviceBusContainer.GetConnectionString()
                    });
            }
        })
        .ConfigureTestServices(services =>
        {
            services.RemoveAll<IGenerateRecipeAI>();
            services.AddScoped(_ => IGenerateRecipeAIBuilder.Build());

            services.RemoveAll<IStorageService>();
            services.AddSingleton<IStorageService>(StorageMock.Object);

            if (_serviceBusContainer?.State == TestcontainersStates.Running)
            {
                services.AddHostedService<DeleteUserAccountConsumer>();
            }
            else
            {
                var consumer = services.SingleOrDefault(service =>
                    service.ImplementationType == typeof(DeleteUserAccountConsumer));

                if (consumer is not null)
                    services.Remove(consumer);
            }
        });
    }

    public async virtual Task InitializeAsync()
    {
        if (_serviceBusContainer is not null)
            await _serviceBusContainer.StartAsync();
        else
            await _msSqlContainer.StartAsync();
        
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
        await dbContext.RefreshTokens.ExecuteDeleteAsync();
        await dbContext.Recipes.ExecuteDeleteAsync();
        await dbContext.Users.ExecuteDeleteAsync();

        await dbContext.Users.AddAsync(_user);
        await dbContext.Recipes.AddAsync(_recipe);

        await dbContext.SaveChangesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        if (_serviceBusContainer is not null)
            await _serviceBusContainer.DisposeAsync();
        
        await _msSqlContainer.DisposeAsync();
        if (_network is not null)
            await _network.DisposeAsync();
    }
}
