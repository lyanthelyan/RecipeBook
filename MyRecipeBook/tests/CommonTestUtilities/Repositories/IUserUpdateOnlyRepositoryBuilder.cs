using Moq;
using MyRecipeBook.Domain.Repositories.User;

namespace CommonTestUtilities.Repositories;

public class IUserUpdateOnlyRepositoryBuilder
{
    public static IUserUpdateOnlyRepository Build(int updatedRows = 1)
    {
        var mock = new Mock<IUserUpdateOnlyRepository>();

        mock.Setup(repository => repository.UpdatePassword(
            It.IsAny<Guid>(),
            It.IsAny<string>()))
            .ReturnsAsync(updatedRows);

        return mock.Object;
    }

}
