using CommonTestUtilities.Entities;
using CommonTestUtilities.Identity;
using CommonTestUtilities.Menssaging;
using CommonTestUtilities.Repositories;
using MyRecipeBook.Application.UseCases.User.DeleteAccount;
using Shouldly;

namespace UseCases.Tests.User.DeleteAccount;

public class DeleteUserAccountUseCaseTests
{
    [Fact]
    public async Task Success()
    {
        var useCase = CreateUseCase();

        await useCase.Execute().ShouldNotThrowAsync();
    }
    
    private static DeleteUserAccountUseCase CreateUseCase()
    {
        var (user, _) = UserBuilder.Build();

        var loggedUser = ILoggedUserBuilder.Build(user);
        var userWriteOnlyRepository = IUserWriteOnlyRepositoriyBuilder.Build();
        var scheduleAccountDeletion = IScheduleAccountDeletionBuilder.Build();

        return new DeleteUserAccountUseCase(loggedUser, userWriteOnlyRepository, scheduleAccountDeletion);
    }
}
