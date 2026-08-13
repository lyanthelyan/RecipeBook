using MyRecipeBook.Communication.Requests;
using Shouldly;
using System.Net;

namespace WebApi.Tests.Recipe.Filter;

public class FilterRecipesInvalidTokenTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "recipes/filter";

    private readonly string _tokenUserNotExistDatabase;

    public FilterRecipesInvalidTokenTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotExistDatabase = factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsInvalid()
    {
        var response = await Post(
            REQUEST_URI,
            new RequestFilterRecipesJson(),
            accessToken: "tokenInvalid");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsMissing()
    {
        var response = await Post(
            REQUEST_URI,
            new RequestFilterRecipesJson(),
            accessToken: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenUserFromAccessTokenDoesNotExist()
    {
        var response = await Post(
            REQUEST_URI,
            new RequestFilterRecipesJson(),
            accessToken: _tokenUserNotExistDatabase);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
