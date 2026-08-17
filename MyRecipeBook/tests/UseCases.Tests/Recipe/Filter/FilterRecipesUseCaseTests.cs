using CommonTestUtilities.Entities;
using CommonTestUtilities.Identity;
using CommonTestUtilities.Repositories;
using MyRecipeBook.Application.Mappings;
using MyRecipeBook.Application.UseCases.Recipe.Filter;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Dtos;
using Shouldly;

namespace UseCases.Tests.Recipe.Filter;

public class FilterRecipesUseCaseTests
{
    [Fact]
    public async Task Success_WhenRequestIsNull()
    {
        MapsterConfiguration.Configure();

        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);

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

        repositoryBuilder.VerifyFilterRecipes(user.Id, new RecipeFilterDto());
    }

    [Fact]
    public async Task Success_WithDefaultRequest()
    {
        MapsterConfiguration.Configure();

        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);
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

        repositoryBuilder.VerifyFilterRecipes(user.Id, new RecipeFilterDto());
    }

    private static FilterRecipesUseCase CreateUseCase(MyRecipeBook.Domain.Entities.User user, IRecipeReadOnlyRepositoryBuilder repositoryBuilder)
    {
        var loggedUser = ILoggedUserBuilder.Build(user);

        return new FilterRecipesUseCase(loggedUser, repositoryBuilder.Build());
    }
}
