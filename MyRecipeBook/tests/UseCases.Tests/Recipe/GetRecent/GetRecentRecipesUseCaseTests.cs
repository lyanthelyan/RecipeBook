using CommonTestUtilities.Entities;
using CommonTestUtilities.Identity;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Storage;
using MyRecipeBook.Application.Mappings;
using MyRecipeBook.Application.UseCases.Recipe.GetRecent;
using Shouldly;

namespace UseCases.Tests.Recipe.GetRecent;

public class GetRecentRecipesUseCaseTests
{
    [Theory]
    [InlineData(true, IStorageServiceBuilder.FakeUrl)]
    [InlineData(false, "")]
    public async Task Sucess(bool hasImage, string expectedUrl)
    {
        MapsterConfiguration.Configure();

        var (user, _) = UserBuilder.Build();

        var recipe = RecipeBuilder.Build(user);
        recipe.HasImage = hasImage;

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
        result.Recipes.ShouldAllBe(recipe => recipe.ImageUrl.Equals(expectedUrl));
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
        var storageServiceBuilder = IStorageServiceBuilder.Build();
        return new GetRecentRecipesUseCase(loggedUser, repository, storageServiceBuilder);
    }
}
