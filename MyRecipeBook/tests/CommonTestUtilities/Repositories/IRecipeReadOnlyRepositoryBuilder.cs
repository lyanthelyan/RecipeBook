using Moq;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.Recipe;

namespace CommonTestUtilities.Repositories;

public class IRecipeReadOnlyRepositoryBuilder
{
    private readonly Mock<IRecipeReadOnlyRepository> _mock;

    public IRecipeReadOnlyRepositoryBuilder()
    {
        _mock = new Mock<IRecipeReadOnlyRepository>();
    }

    public IRecipeReadOnlyRepositoryBuilder GetById(Recipe recipe)
    {
        _mock.Setup(repository => repository.GetById(recipe.Id, recipe.UserId))
            .ReturnsAsync(recipe);
        return this;
    }

    public IRecipeReadOnlyRepositoryBuilder GetRecentRecipes(Guid userId, IList<Recipe> recipes)
    {
        _mock.Setup(repository => repository.GetRecentRecipes(userId))
            .ReturnsAsync(recipes.Select(recipe => new RecipeSummaryDto(recipe.Id, recipe.Title)).ToList());
        return this;
    }

    public IRecipeReadOnlyRepositoryBuilder FilterRecipes(IList<Recipe> recipes)
    {
        _mock.Setup(repository => repository.FilterRecipes(It.IsAny<Guid>(), It.IsAny<RecipeFilterDto>()))
            .ReturnsAsync(recipes.Select(recipe => new RecipeSummaryDto(recipe.Id, recipe.Title)).ToList());
        return this;
    }

    public void VerifyFilterRecipes(Guid userId, RecipeFilterDto expectedFilter)
    {
        _mock.Verify(repository => repository.FilterRecipes(
            userId,
            It.Is<RecipeFilterDto>(filter =>
                filter.SearchTerm == expectedFilter.SearchTerm &&
                filter.CookTime == expectedFilter.CookTime &&
                filter.DishTypes.SequenceEqual(expectedFilter.DishTypes))),
            Times.Once);
    }

    public IRecipeReadOnlyRepository Build()
    {   
        return _mock.Object;
    }
}
