using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.Recipe;

namespace CommonTestUtilities.Repositories;

public class IRecipeWriteOnlyRepositoryBuilder
{
    public static IRecipeWriteOnlyRepository Build(Recipe? recipe = null)
    {
        var mock = new Mock<IRecipeWriteOnlyRepository>();

        if (recipe is not null)
        {
            mock.Setup(repository => repository.DeleteById(recipe.Id, recipe.UserId))
                .ReturnsAsync(true);
        }

        return mock.Object;
    }
}
