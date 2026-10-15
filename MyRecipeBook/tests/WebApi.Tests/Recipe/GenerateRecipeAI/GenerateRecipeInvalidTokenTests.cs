using CommonTestUtilities.Requests;
using Shouldly;
using System.Net;

namespace WebApi.Tests.Recipe.GenerateRecipeAi;

public class GenerateRecipeInvalidTokenTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "recipes/generate";

    private readonly string _tokenUserNotFoundInDatabase;

    public GenerateRecipeInvalidTokenTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotFoundInDatabase = factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsInvalid()
    {
        var request = RequestGenerateRecipeJsonBuilder.Build();

        var response = await Post(
            requestUri: REQUEST_URI,
            request,
            accessToken: "tokenInvalid");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsMissing()
    {
        var request = RequestGenerateRecipeJsonBuilder.Build();

        var response = await Post(
            requestUri: REQUEST_URI,
            request,
            accessToken: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenUserFromAccessTokenDoesNotExist()
    {
        var request = RequestGenerateRecipeJsonBuilder.Build();

        var response = await Post(
            requestUri: REQUEST_URI,
            request,
            accessToken: _tokenUserNotFoundInDatabase);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}