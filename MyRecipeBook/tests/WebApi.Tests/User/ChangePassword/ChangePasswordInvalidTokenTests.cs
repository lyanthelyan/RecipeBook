using CommonTestUtilities.Requests;
using Shouldly;
using System.Net;
using WebApi.Tests.Resources;

namespace WebApi.Tests.User.ChangePassword;

public class ChangePasswordInvalidTokenTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "/users/password";
    private readonly string _tokenUserNotExistDatabase;
    public ChangePasswordInvalidTokenTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotExistDatabase = factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task Error_Token_Invalid()
    {
        var request = RequestChangePasswordJsonBuilder.Build();
        var response = await Put(REQUEST_URI, request, accessToken: "invalidToken");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Error_Without_Token()
    {
        var request = RequestChangePasswordJsonBuilder.Build();
        var response = await Put(REQUEST_URI, request, accessToken: string.Empty);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Error_Token_With_User_NotFound()
    {
        var request = RequestChangePasswordJsonBuilder.Build();
        var response = await Put(REQUEST_URI, request, _tokenUserNotExistDatabase);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
