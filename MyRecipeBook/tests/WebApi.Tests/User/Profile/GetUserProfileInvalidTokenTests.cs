using System.Net;
using Shouldly;
using WebApi.Tests.Resources;

namespace WebApi.Tests.User.Profile;

public class GetUserProfileAuthenticationTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "/users";

    private readonly string _tokenUserNotFoundInDatabase;

    public GetUserProfileAuthenticationTests(
        MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotFoundInDatabase =
            factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        var response = await Get(
            REQUEST_URI,
            accessToken: "invalid-token");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var response = await Get(
            REQUEST_URI,
            accessToken: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenUserDoesNotExist()
    {
        var response = await Get(
            REQUEST_URI,
            accessToken: _tokenUserNotFoundInDatabase);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}