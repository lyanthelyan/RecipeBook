using CommonTestUtilities.Requests;
using Shouldly;
using System.Net;

namespace WebApi.Tests.Recipe.Register;

public class RegisterRecipeAuthenticationTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "/recipes";

    private readonly string _tokenUserNotFoundInDatabase;

    public RegisterRecipeAuthenticationTests(
        MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotFoundInDatabase =
            factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        var request = RequestRecipeJsonBuilder.Build();

        var response = await Post(
            REQUEST_URI,
            request,
            accessToken: "invalid-token");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var request = RequestRecipeJsonBuilder.Build();

        var response = await Post(
            REQUEST_URI,
            request,
            accessToken: string.Empty);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenUserDoesNotExist()
    {
        var request = RequestRecipeJsonBuilder.Build();

        var response = await Post(
            REQUEST_URI,
            request,
            accessToken: _tokenUserNotFoundInDatabase);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
