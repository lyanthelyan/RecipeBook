using CommonTestUtilities.Requests;
using Shouldly;
using System.Net;

namespace WebApi.Tests.Recipe.Update;

public class UpdateRecipeByIdInvalidTokenTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "recipes";

    private readonly string _tokenUserNotExistDatabase;

    public UpdateRecipeByIdInvalidTokenTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotExistDatabase = factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsInvalid()
    {
        var request = RequestRecipeJsonBuilder.Build();

        var response = await Put(
            $"{REQUEST_URI}/{Guid.CreateVersion7()}",
            request,
            accessToken: "tokenInvalid");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsMissing()
    {
        var request = RequestRecipeJsonBuilder.Build();

        var response = await Put(
            $"{REQUEST_URI}/{Guid.CreateVersion7()}",
            request,
            accessToken: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenUserFromAccessTokenDoesNotExist()
    {
        var request = RequestRecipeJsonBuilder.Build();

        var response = await Put(
            $"{REQUEST_URI}/{Guid.CreateVersion7()}",
            request,
            accessToken: _tokenUserNotExistDatabase);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
