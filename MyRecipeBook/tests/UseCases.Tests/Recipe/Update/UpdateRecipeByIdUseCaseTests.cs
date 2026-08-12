using CommonTestUtilities.Entities;
using CommonTestUtilities.Identity;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using MyRecipeBook.Application.Mappings;
using MyRecipeBook.Application.UseCases.Recipe.Update;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;
using Shouldly;
using System.Net;

namespace UseCases.Tests.Recipe.Update;

public class UpdateRecipeByIdUseCaseTests
{
    [Fact]
    public async Task Success()
    {
        MapsterConfiguration.Configure();

        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);
        var request = RequestRecipeJsonBuilder.Build();

        var useCase = CreateUseCase(user, recipe);

        await useCase.Execute(recipe.Id, request).ShouldNotThrowAsync();

        recipe.Title.ShouldBe(request.Title);
        recipe.CookTime.ShouldBe((MyRecipeBook.Domain.Enums.CookTime)request.CookTime);
        recipe.Ingredients.Count.ShouldBe(request.Ingredients.Count);
        recipe.Instructions.Count.ShouldBe(request.Instructions.Count);
        recipe.DishTypes.Count.ShouldBe(request.DishTypes.Count);
    }

    [Fact]
    public async Task Validate_ShouldThrowException_WhenRecipeNotFound()
    {
        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);
        var request = RequestRecipeJsonBuilder.Build();

        var useCase = CreateUseCase(user, recipe);

        var exception = await useCase.Execute(Guid.CreateVersion7(), request).ShouldThrowAsync<NotFoundException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.NotFound);

        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages =>
        {
            errorMessages.Count.ShouldBe(1);
            errorMessages.ShouldContain(ResourceMessagesException.VALIDATION_RECIPE_NOT_FOUND);
        });
    }

    [Fact]
    public async Task Validate_ShouldThrowException_WhenTitleIsEmpty()
    {
        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);
        var request = RequestRecipeJsonBuilder.Build();
        request.Title = string.Empty;

        var useCase = CreateUseCase(user, recipe);

        var exception = await useCase.Execute(recipe.Id, request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages =>
        {
            errorMessages.Count.ShouldBe(1);
            errorMessages.ShouldContain(ResourceMessagesException.VALIDATION_RECIPE_TITLE_REQUIRED);
        });
    }

    private static UpdateRecipeByIdUseCase CreateUseCase(MyRecipeBook.Domain.Entities.User user, MyRecipeBook.Domain.Entities.Recipe recipe)
    {
        var loggedUser = ILoggedUserBuilder.Build(user);
        var repository = IRecipeUpdateOnlyRepositoryBuilder.Build(recipe);
        var unitOfWork = IUnitOfWorkBuilder.Build();

        return new UpdateRecipeByIdUseCase(loggedUser, repository, unitOfWork);
    }
}
