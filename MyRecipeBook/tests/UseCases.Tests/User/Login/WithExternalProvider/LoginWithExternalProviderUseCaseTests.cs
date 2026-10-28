using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using MyRecipeBook.Application.UseCases.Login.WithExternalProvider;
using MyRecipeBook.Domain.Dtos;
using Shouldly;

namespace UseCases.Tests.User.Login.WithExternalProvider;

public class LoginWithExternalProviderUseCaseTests
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);

        var code = await useCase.Execute(new ExternalLoginInfo(user.Name, user.Email));

        code.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Success_WhenUserDoesNotExist()
    {
        var useCase = CreateUseCase();

        var code = await useCase.Execute(new ExternalLoginInfo("user", "user@gmail.com"));

        code.ShouldNotBeNullOrEmpty();
    }

    private static LoginWithExternalProviderUseCase CreateUseCase(
    MyRecipeBook.Domain.Entities.User? user = null)
    {
        var userReadOnlyRepositoryBuilder = new IUserReadOnlyRepositoryBuilder();

        if (user is not null)
            userReadOnlyRepositoryBuilder.GetByEmail(user);

        var userWriteOnlyRepository = IUserWriteOnlyRepositoriyBuilder.Build();
        var verificationCodeWriteOnlyRepository = IVerificationCodeWriteOnlyRepositoryBuilder.Build();
        var unitOfWork = IUnitOfWorkBuilder.Build();

        return new LoginWithExternalProviderUseCase(
            userReadOnlyRepositoryBuilder.Build(),
            userWriteOnlyRepository,
            verificationCodeWriteOnlyRepository,
            unitOfWork);
    }
}
