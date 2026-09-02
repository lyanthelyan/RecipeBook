using CommonTestUtilities.Files;
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

namespace WebApi.Tests.Recipe.Register;

public class RegisterRecipeTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "/recipes";
    private const string FILE_FIELD_NAME = "recipeIllustration";
    private readonly UserIdentityManager _user1;

    public RegisterRecipeTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _user1 = factory.User1;
    }

    [Fact]
    public async Task Success_WithoutImage()
    {
        // Arrange
        var request = RequestRecipeJsonBuilder.Build();

        // Act
        var response = await PostFormData(REQUEST_URI, request, accessToken: _user1.GetAccessToken(), fileFieldName: FILE_FIELD_NAME);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        var recipeId = responseData.RootElement.GetProperty("id").GetGuid();
        recipeId.ShouldNotBe(Guid.Empty);
        responseData.RootElement.GetProperty("title").GetString().ShouldBe(request.Title);
        responseData.RootElement.GetProperty("imageUrl").GetString().ShouldBeNullOrEmpty();


        var recipe = await DbContext.Recipes
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Instructions)
            .Include(recipe => recipe.DishTypes)
            .SingleAsync(recipe => recipe.Id == recipeId);

        recipe.ShouldSatisfyAllConditions(registeredRecipe =>
        {
            registeredRecipe.Active.ShouldBeTrue();
            registeredRecipe.UserId.ShouldBe(_user1.GetId());
            registeredRecipe.Title.ShouldBe(request.Title);
            registeredRecipe.CookTime.ShouldBe((MyRecipeBook.Domain.Enums.CookTime)request.CookTime);
            registeredRecipe.Ingredients.Count.ShouldBe(request.Ingredients.Count);
            registeredRecipe.Instructions.Count.ShouldBe(request.Instructions.Count);
            registeredRecipe.DishTypes.Count.ShouldBe(request.DishTypes.Count);
        });

        request.Ingredients.ShouldAllBe(ingredient => recipe.Ingredients.Any(registeredIngredient => registeredIngredient.Item.Equals(ingredient)));
        
        request.Instructions.ShouldAllBe(instruction => recipe.Instructions.Any(registeredInstruction =>
            registeredInstruction.Order == instruction.Order &&
            registeredInstruction.Description.Equals(instruction.Description)));
        
        request.DishTypes.ShouldAllBe(dishType => recipe.DishTypes.Any(registeredDishType =>
            registeredDishType.Type == (MyRecipeBook.Domain.Enums.DishType)dishType));

        var existImageInStorage = await BlobServiceClient.GetBlobContainerClient(_user1.GetId().ToString())
            .GetBlobClient(recipeId.ToString())
            .ExistsAsync();
        existImageInStorage.Value.ShouldBeFalse();
    }
    [Fact]
    public async Task Success_WithImage()
    {
        // Arrange
        var request = RequestRecipeJsonBuilder.Build();

        // Act
        var response = await PostFormData(REQUEST_URI, request, accessToken: _user1.GetAccessToken(), file:FileBuilder.GetPng(), fileFieldName: FILE_FIELD_NAME);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        var recipeId = responseData.RootElement.GetProperty("id").GetGuid();
        recipeId.ShouldNotBe(Guid.Empty);
        responseData.RootElement.GetProperty("title").GetString().ShouldBe(request.Title);
        responseData.RootElement.GetProperty("imageUrl").GetString().ShouldNotBeNullOrEmpty();

        var recipe = await DbContext.Recipes
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Instructions)
            .Include(recipe => recipe.DishTypes)
            .SingleAsync(recipe => recipe.Id == recipeId);

        recipe.ShouldSatisfyAllConditions(registeredRecipe =>
        {
            registeredRecipe.Active.ShouldBeTrue();
            registeredRecipe.UserId.ShouldBe(_user1.GetId());
            registeredRecipe.Title.ShouldBe(request.Title);
            registeredRecipe.CookTime.ShouldBe((MyRecipeBook.Domain.Enums.CookTime)request.CookTime);
            registeredRecipe.Ingredients.Count.ShouldBe(request.Ingredients.Count);
            registeredRecipe.Instructions.Count.ShouldBe(request.Instructions.Count);
            registeredRecipe.DishTypes.Count.ShouldBe(request.DishTypes.Count);

        });
        request.Ingredients.ShouldAllBe(ingredient => recipe.Ingredients.Any(registeredIngredient => registeredIngredient.Item.Equals(ingredient)));
        
        request.Instructions.ShouldAllBe(instruction => recipe.Instructions.Any(registeredInstruction =>
            registeredInstruction.Order == instruction.Order &&
            registeredInstruction.Description.Equals(instruction.Description)));
        
        request.DishTypes.ShouldAllBe(dishType => recipe.DishTypes.Any(registeredDishType =>
            registeredDishType.Type == (MyRecipeBook.Domain.Enums.DishType)dishType));

        var existImageInStorage = await BlobServiceClient.GetBlobContainerClient(_user1.GetId().ToString())
            .GetBlobClient(recipeId.ToString())
            .ExistsAsync();
        existImageInStorage.Value.ShouldBeTrue();
    }
    [Theory]
    [ClassData(typeof(CultureInlineData))]
    public async Task Error_WhenImageIsTxt(string culture)
    {
        var request = RequestRecipeJsonBuilder.Build();

        var response = await PostFormData(REQUEST_URI, request, accessToken: _user1.GetAccessToken(), file: FileBuilder.GetTxt(), culture: culture, fileFieldName: FILE_FIELD_NAME);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        var expectedErrorMessage = ResourceMessagesException.ResourceManager.GetString("VALIDATION_ONLY_IMAGES_ACCEPTED", new CultureInfo(culture));

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        errors.ShouldSatisfyAllConditions(errorsList =>
        {
            errorsList.Count().ShouldBe(1);
            errorsList.ShouldContain(error => error
                .GetString()
                .IsNotEmpty()
                && error
                .GetString()!
                .Equals(expectedErrorMessage));
        });

        var recipeExists = await DbContext.Recipes.AnyAsync(recipe =>
            recipe.Active &&
            recipe.UserId == _user1.GetId() &&
            recipe.Title.Equals(request.Title));

        recipeExists.ShouldBeFalse();
    }

    [Theory]
    [ClassData(typeof(CultureInlineData))]
    public async Task Validate_ShouldBeAnErrorResponse_WhenThereIsNoInstruction(string culture)
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions = [];

        var response = await PostFormData(REQUEST_URI, request, accessToken: _user1.GetAccessToken(), culture: culture, fileFieldName: FILE_FIELD_NAME);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        var expectedErrorMessage = ResourceMessagesException.ResourceManager.GetString("VALIDATION_AT_LEAST_ONE_INSTRUCTION_REQUIRED", new CultureInfo(culture));

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        errors.ShouldSatisfyAllConditions(errorsList =>
        {
            errorsList.Count().ShouldBe(1);
            errorsList.ShouldContain(error => error
                .GetString()
                .IsNotEmpty()
                && error
                .GetString()!
                .Equals(expectedErrorMessage));
        });

        var recipeExists = await DbContext.Recipes.AnyAsync(recipe =>
            recipe.Active &&
            recipe.UserId == _user1.GetId() &&
            recipe.Title.Equals(request.Title));

        recipeExists.ShouldBeFalse();
    }
}
