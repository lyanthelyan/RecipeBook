using Moq;
using MyRecipeBook.Domain.Repositories.User;
namespace CommonTestUtilities.Repositories;

public class IUserWriteOnlyRepositoriyBuilder
{
    public static IUserWriteOnlyRepository Build()
    {
        var mock = new Mock<IUserWriteOnlyRepository>();

        return mock.Object;

    }
}
