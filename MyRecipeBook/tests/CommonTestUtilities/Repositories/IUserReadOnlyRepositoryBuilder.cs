using Moq;
using MyRecipeBook.Domain.Repositories.User;

namespace CommonTestUtilities.Repositories;

public class IUserReadOnlyRepositoryBuilder
{
    private readonly Mock<IUserReadOnlyRepository> _mock;
    
    public IUserReadOnlyRepositoryBuilder()
    {
       _mock =  new Mock<IUserReadOnlyRepository>();
    }

    public IUserReadOnlyRepository Build()
    {
        return _mock.Object;
    }

    public void ExistActiveUserWithEmail(string email)
    {
        _mock.Setup(repository => repository.ExistActiveUserWithEmail(email)).ReturnsAsync(true);
    }
}
