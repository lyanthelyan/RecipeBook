using CommonTestUtilities.Entities;
using CommonTestUtilities.Identity;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Storage;
using MyRecipeBook.Application.Mappings;
using MyRecipeBook.Application.UseCases.Recipe.Filter;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Dtos;
using Shouldly;

namespace UseCases.Tests.Recipe.Filter;

public class FilterRecipesUseCaseTests
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

        var repositoryBuilder = new IRecipeReadOnlyRepositoryBuilder()
            .FilterRecipes([recipe]);
        var useCase = CreateUseCase(user, repositoryBuilder);

        var result = await useCase.Execute(null);

        result.ShouldNotBeNull();
        result.Recipes.ShouldSatisfyAllConditions(recipes =>
        {
            recipes.Count.ShouldBe(1);
            recipes.ShouldContain(responseRecipe =>
                responseRecipe.Id == recipe.Id &&
                responseRecipe.Title.Equals(recipe.Title));
        });
        result.Recipes.ShouldAllBe(recipe => recipe.ImageUrl.Equals(expectedUrl));

        repositoryBuilder.VerifyFilterRecipes(user.Id, new RecipeFilterDto());
    }

    [Theory]
    [InlineData(true, IStorageServiceBuilder.FakeUrl)]
    [InlineData(false, "")]
    public async Task Success_WithDefaultRequest(bool hasImage, string expectedUrl)
    {
        MapsterConfiguration.Configure();

        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);
        recipe.HasImage = hasImage;
        var request = new RequestFilterRecipesJson();
        

        var repositoryBuilder = new IRecipeReadOnlyRepositoryBuilder()
            .FilterRecipes([recipe]);
        var useCase = CreateUseCase(user, repositoryBuilder);

        var result = await useCase.Execute(request);

        result.ShouldNotBeNull();
        result.Recipes.ShouldSatisfyAllConditions(recipes =>
        {
            recipes.Count.ShouldBe(1);
            recipes.ShouldContain(responseRecipe =>
                responseRecipe.Id == recipe.Id &&
                responseRecipe.Title.Equals(recipe.Title));
        });
        result.Recipes.ShouldAllBe(recipe => recipe.ImageUrl.Equals(expectedUrl));
        repositoryBuilder.VerifyFilterRecipes(user.Id, new RecipeFilterDto());
    }

    private static FilterRecipesUseCase CreateUseCase(MyRecipeBook.Domain.Entities.User user, IRecipeReadOnlyRepositoryBuilder repositoryBuilder)
    {
        var loggedUser = ILoggedUserBuilder.Build(user);
        var storageServiceBuilder = IStorageServiceBuilder.Build();
        return new FilterRecipesUseCase(loggedUser, repositoryBuilder.Build(), storageServiceBuilder);
    }
}
