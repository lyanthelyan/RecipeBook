using CommonTestUtilities.Files;
using MyRecipeBook.Domain.Entities;
using Shouldly;
using System.Net;

namespace WebApi.Tests.User.ChangeProfilePictureTests;

public class ChangeProfilePictureInvalidTokenTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "users/profile-picture";
    private const string FILE_FIELD_NAME = "profilePicture";

    private readonly string _tokenUserNotFoundInDatabase;

    public ChangeProfilePictureInvalidTokenTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _tokenUserNotFoundInDatabase = factory.TokenUserNotFoundInDatabase;
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsInvalid()
    {
        var response = await PutFormData(
            REQUEST_URI,
            FileBuilder.GetPng(),
            accessToken: "invalid-token",
            FILE_FIELD_NAME);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenAccessTokenIsMissing()
    {
        var response = await PutFormData(
            REQUEST_URI,
            FileBuilder.GetPng(),
            accessToken: string.Empty,
            FILE_FIELD_NAME);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ShouldBeAnErrorResponse_WhenUserFromAccessTokenDoesNotExist()
    {
        var response = await PutFormData(
            REQUEST_URI,
            FileBuilder.GetPng(),
            accessToken: _tokenUserNotFoundInDatabase,
            FILE_FIELD_NAME);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
