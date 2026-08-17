using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Exception;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Tests.InlineData;
using WebApi.Tests.Resources;

namespace WebApi.Tests.Recipe.Update;

public class UpdateRecipeByIdTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "/recipes";
    private readonly UserIdentityManager _user1;

    public UpdateRecipeByIdTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _user1 = factory.User1;
    }

    [Fact]
    public async Task Success()
    {
        var recipe = _user1.GetRecipe();
        var request = RequestRecipeJsonBuilder.Build();

        var response = await Put(
            $"{REQUEST_URI}/{recipe.Id}",
            request,
            accessToken: _user1.GetAccessToken());

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var updatedRecipe = await DbContext.Recipes
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Instructions)
            .Include(recipe => recipe.DishTypes)
            .SingleAsync(recipe => recipe.Id == _user1.GetRecipe().Id);

        updatedRecipe.ShouldSatisfyAllConditions(recipeFromDatabase =>
        {
            recipeFromDatabase.Active.ShouldBeTrue();
            recipeFromDatabase.UserId.ShouldBe(_user1.GetId());
            recipeFromDatabase.Title.ShouldBe(request.Title);
            recipeFromDatabase.CookTime.ShouldBe((MyRecipeBook.Domain.Enums.CookTime)request.CookTime);
            recipeFromDatabase.Ingredients.Count.ShouldBe(request.Ingredients.Count);
            recipeFromDatabase.Instructions.Count.ShouldBe(request.Instructions.Count);
            recipeFromDatabase.DishTypes.Count.ShouldBe(request.DishTypes.Count);
        });

        request.Ingredients.ShouldAllBe(ingredient => updatedRecipe.Ingredients.Any(updatedIngredient => updatedIngredient.Item.Equals(ingredient)));
        request.Instructions.ShouldAllBe(instruction => updatedRecipe.Instructions.Any(updatedInstruction =>
            updatedInstruction.Order == instruction.Order &&
            updatedInstruction.Description.Equals(instruction.Description)));
        request.DishTypes.ShouldAllBe(dishType => updatedRecipe.DishTypes.Any(updatedDishType =>
            updatedDishType.Type == (MyRecipeBook.Domain.Enums.DishType)dishType));
    }

    [Theory]
    [ClassData(typeof(CultureInlineData))]
    public async Task Validate_ShouldBeAnErrorResponse_WhenRecipeNotFound(string culture)
    {
        var request = RequestRecipeJsonBuilder.Build();

        var response = await Put(
            $"{REQUEST_URI}/{Guid.CreateVersion7()}",
            request,
            accessToken: _user1.GetAccessToken(),
            culture: culture);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedErrorMessage = ResourceMessagesException.ResourceManager.GetString("VALIDATION_RECIPE_NOT_FOUND", new CultureInfo(culture));

        errors.ShouldSatisfyAllConditions(errorsList =>
        {
            errorsList.Count().ShouldBe(1);
            errorsList.ShouldContain(error =>
                error.GetString().IsNotEmpty() &&
                error.GetString()!.Equals(expectedErrorMessage));
        });
    }

    [Theory]
    [ClassData(typeof(CultureInlineData))]
    public async Task Validate_ShouldBeAnErrorResponse_WhenTitleIsEmpty(string culture)
    {
        var recipe = _user1.GetRecipe();
        var request = RequestRecipeJsonBuilder.Build();
        request.Title = string.Empty;

        var response = await Put(
            $"{REQUEST_URI}/{recipe.Id}",
            request,
            accessToken: _user1.GetAccessToken(),
            culture: culture);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedErrorMessage = ResourceMessagesException.ResourceManager.GetString("VALIDATION_RECIPE_TITLE_REQUIRED", new CultureInfo(culture));

        errors.ShouldSatisfyAllConditions(errorsList =>
        {
            errorsList.Count().ShouldBe(1);
            errorsList.ShouldContain(error =>
                error.GetString().IsNotEmpty() &&
                error.GetString()!.Equals(expectedErrorMessage));
        });
    }
}
