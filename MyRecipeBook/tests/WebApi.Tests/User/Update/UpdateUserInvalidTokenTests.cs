using CommonTestUtilities.Requests;
using System.Net;
using Shouldly;

namespace WebApi.Tests.User.Update;

public class UpdateUserAuthenticationTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "/users/profile";

    private readonly string _tokenUserNotFoundInDatabase;

    public UpdateUserAuthenticationTests(
        MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotFoundInDatabase =
            factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        var request = RequestUpdateUserJsonBuilder.Build();

        var response = await Put(
            REQUEST_URI,
            request,
            accessToken: "invalid-token");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var request = RequestUpdateUserJsonBuilder.Build();

        var response = await Put(
            REQUEST_URI,
            request,
            accessToken: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenUserDoesNotExist()
    {
        var request = RequestUpdateUserJsonBuilder.Build();

        var response = await Put(
            REQUEST_URI,
            request,
            accessToken: _tokenUserNotFoundInDatabase);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}