using Shouldly;
using CommonTestUtilities.Files;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Exception;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Tests.InlineData;
using WebApi.Tests.Resources;
namespace WebApi.Tests.User.ChangeProfilePictureTests;

public class ChangeProfilePictureTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "users/profile-picture";
    private const string FILE_FIELD_NAME = "profilePicture";

    private readonly UserIdentityManager _user1;

    public ChangeProfilePictureTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _user1 = factory.User1;
    }

    [Fact]
    public async Task Success()
    {
        var response = await PutFormData(
            REQUEST_URI,
            FileBuilder.GetPng(),
            accessToken: _user1.GetAccessToken(),
            FILE_FIELD_NAME);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Theory]
    [ClassData(typeof(CultureInlineData))]
    public async Task Validate_ShouldBeAnErrorResponse_WhenImageIsInvalid(string culture)
    {
        var response = await PutFormData(
            REQUEST_URI,
            FileBuilder.GetTxt(),
            accessToken: _user1.GetAccessToken(),
            FILE_FIELD_NAME,
            culture);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        var expectedErrorMessage = ResourceMessagesException.ResourceManager.GetString(
            "VALIDATION_ONLY_IMAGES_ACCEPTED",
            new CultureInfo(culture));

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        errors.ShouldSatisfyAllConditions(errorsList =>
        {
            errorsList.Count().ShouldBe(1);
            errorsList.ShouldContain(error => error
                .GetString()
                .IsNotEmpty()
                && error
                .GetString()!
                .Equals(expectedErrorMessage));
        });
    }
}
