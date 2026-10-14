using Moq;
using MyRecipeBook.Domain.Repositories;

namespace CommonTestUtilities.Repositories;

public class IUnitOfWorkBuilder
{
    public static IUnitOfWork Build()
    {
        var mock = new Mock<IUnitOfWork>();

        mock.Setup(unit => unit.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> operation) => operation());

        return mock.Object;
    }
}
