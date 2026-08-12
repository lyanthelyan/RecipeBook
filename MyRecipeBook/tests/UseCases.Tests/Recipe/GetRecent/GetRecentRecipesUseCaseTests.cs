using CommonTestUtilities.Entities;
using CommonTestUtilities.Identity;
using CommonTestUtilities.Repositories;
using MyRecipeBook.Application.Mappings;
using MyRecipeBook.Application.UseCases.Recipe.GetRecent;
using Shouldly;

namespace UseCases.Tests.Recipe.GetRecent;

public class GetRecentRecipesUseCaseTests
{
    [Fact]
    public async Task Success()
    {
        MapsterConfiguration.Configure();

        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);

        var useCase = CreateUseCase(user, [recipe]);

        var result = await useCase.Execute();

        result.ShouldNotBeNull();
        result.Recipes.ShouldSatisfyAllConditions(recipes =>
        {
            recipes.Count.ShouldBe(1);
            recipes.ShouldContain(responseRecipe =>
                responseRecipe.Id == recipe.Id &&
                responseRecipe.Title.Equals(recipe.Title));
        });
    }

    [Fact]
    public async Task Success_WhenThereAreNoRecipes()
    {
        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user, []);

        var result = await useCase.Execute();

        result.ShouldNotBeNull();
        result.Recipes.ShouldBeEmpty();
    }

    private static GetRecentRecipesUseCase CreateUseCase(MyRecipeBook.Domain.Entities.User user, IList<MyRecipeBook.Domain.Entities.Recipe> recipes)
    {
        var loggedUser = ILoggedUserBuilder.Build(user);
        var repository = new IRecipeReadOnlyRepositoryBuilder()
            .GetRecentRecipes(user.Id, recipes)
            .Build();

        return new GetRecentRecipesUseCase(loggedUser, repository);
    }
}
