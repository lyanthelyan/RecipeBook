using Shouldly;
using System.Net;

namespace WebApi.Tests.Recipe.GetRecent;

public class GetRecentRecipesInvalidTokenTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "recipes/recent";

    private readonly string _tokenUserNotExistDatabase;

    public GetRecentRecipesInvalidTokenTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotExistDatabase = factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsInvalid()
    {
        var response = await Get(
            REQUEST_URI,
            accessToken: "tokenInvalid");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsMissing()
    {
        var response = await Get(
            REQUEST_URI,
            accessToken: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenUserFromAccessTokenDoesNotExist()
    {
        var response = await Get(
            REQUEST_URI,
            accessToken: _tokenUserNotExistDatabase);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
