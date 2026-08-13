using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Extensions;
using Shouldly;
using System.Net;
using System.Text.Json;
using WebApi.Tests.Resources;

namespace WebApi.Tests.Recipe.Filter;

public class FilterRecipesTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "/recipes/filter";
    private readonly UserIdentityManager _user1;

    public FilterRecipesTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _user1 = factory.User1;
    }

    [Fact]
    public async Task Success()
    {
        var recipe = _user1.GetRecipe();
        var request = new RequestFilterRecipesJson
        {
            SearchTerm = recipe.Title,
            CookTime = (MyRecipeBook.Communication.Enums.CookTime)recipe.CookTime,
            DishTypes = recipe.DishTypes
                .Select(dishType => (MyRecipeBook.Communication.Enums.DishType)dishType.Type)
                .ToList()
        };

        var response = await Post(
            REQUEST_URI,
            request,
            accessToken: _user1.GetAccessToken());

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        var recipes = responseData.RootElement
            .GetProperty("recipes")
            .EnumerateArray();

        recipes.ShouldSatisfyAllConditions(responseRecipes =>
        {
            responseRecipes.Count().ShouldBe(1);
            responseRecipes.ShouldContain(responseRecipe =>
                responseRecipe.GetProperty("id").GetGuid() == recipe.Id &&
                responseRecipe.GetProperty("title").GetString().IsNotEmpty() &&
                responseRecipe.GetProperty("title").GetString()!.Equals(recipe.Title));
        });
    }
}
