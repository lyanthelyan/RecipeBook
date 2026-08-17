using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.Recipe;

namespace CommonTestUtilities.Repositories;

public class IRecipeUpdateOnlyRepositoryBuilder
{
    public static IRecipeUpdateOnlyRepository Build(Recipe? recipe = null)
    {
        var mock = new Mock<IRecipeUpdateOnlyRepository>();

        if (recipe is not null)
        {
            mock.Setup(repository => repository.GetById(recipe.Id, recipe.UserId))
                .ReturnsAsync(recipe);
        }

        return mock.Object;
    }
}
