using CommonTestUtilities.Entities;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Tests.InlineData;
using WebApi.Tests.Resources;

namespace WebApi.Tests.Token.UseRefreshToken;

public class UseRefreshTokenTests : BaseIntegrationTest
{
    private const string REQUEST_URI = "/authentication/token/refresh";
    private const string PROFILE_URI = "/users";
    private readonly UserIdentityManager _user1;

    public UseRefreshTokenTests(MyRecipeBookApplicationFactory factory) : base(factory)
    {
        _user1 = factory.User1;
    }

    [Fact]
    public async Task ShouldReturnAndPersistNewTokens_WhenUsingTokenFromLogin()
    {
        var loginRequest = new RequestLoginJson
        {
            Email = _user1.GetEmail(),
            Password = _user1.GetPassword()
        };
        var loginResponse = await Post("/authentication/login", loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var loginData = await ReadResponse(loginResponse);
        var originalToken = loginData.RootElement.GetProperty("tokens").GetProperty("refreshToken").GetString()!;
        originalToken.ShouldNotBeNullOrEmpty();
        var originalTokenExists = await DbContext.RefreshTokens.AnyAsync(token =>
            token.UserId == _user1.GetId() && token.Value == originalToken);
        originalTokenExists.ShouldBeTrue();

        var request = RequestNewTokenJsonBuilder.Build(originalToken);
        var beforeRefresh = DateTimeOffset.UtcNow;
        var response = await Post(REQUEST_URI, request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tokens = await ReadTokens(response);
        tokens.AccessToken.ShouldNotBeNullOrEmpty();
        tokens.RefreshToken.ShouldNotBeNullOrEmpty();
        tokens.RefreshToken.ShouldNotBe(originalToken);

        var savedToken = await DbContext.RefreshTokens.AsNoTracking().SingleAsync();
        savedToken.Value.ShouldBe(tokens.RefreshToken);
        savedToken.UserId.ShouldBe(_user1.GetId());
        savedToken.Active.ShouldBeTrue();
        savedToken.CreatedOn.ShouldBeGreaterThanOrEqualTo(beforeRefresh);
        savedToken.CreatedOn.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ShouldAuthenticateUser_WhenUsingRenewedAccessToken()
    {
        var token = await AddRefreshToken();
        var tokens = await RenewToken(token.Value);

        var response = await Get(PROFILE_URI, tokens.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var responseData = await ReadResponse(response);
        responseData.RootElement.GetProperty("email").GetString().ShouldBe(_user1.GetEmail());
    }

    [Fact]
    public async Task ShouldRotateCurrentToken_AfterRejectingReuseOfPreviousToken()
    {
        var token = await AddRefreshToken();
        var tokens = await RenewToken(token.Value);
        var savedToken = await DbContext.RefreshTokens.AsNoTracking().SingleAsync();
        var reuseResponse = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(token.Value));
        await AssertUnauthorized(reuseResponse);
        var request = RequestNewTokenJsonBuilder.Build(tokens.RefreshToken);

        var response = await Post(REQUEST_URI, request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var nextTokens = await ReadTokens(response);
        nextTokens.RefreshToken.ShouldNotBeNullOrEmpty();
        nextTokens.RefreshToken.ShouldNotBe(tokens.RefreshToken);
        nextTokens.RefreshToken.ShouldNotBe(token.Value);
        var nextSavedToken = await DbContext.RefreshTokens.AsNoTracking().SingleAsync();
        nextSavedToken.Value.ShouldBe(nextTokens.RefreshToken);
        nextSavedToken.UserId.ShouldBe(_user1.GetId());
        nextSavedToken.Id.ShouldNotBe(savedToken.Id);

        var profileResponse = await Get(PROFILE_URI, nextTokens.AccessToken);
        profileResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task ShouldReturnUnauthorized_WhenReusingTokenAfterRotation(int rotationCount)
    {
        var token = await AddRefreshToken();
        var previousToken = token.Value;
        var currentToken = token.Value;
        for (var rotation = 0; rotation < rotationCount; rotation++)
        {
            previousToken = currentToken;
            var tokens = await RenewToken(currentToken);
            currentToken = tokens.RefreshToken;
        }
        var savedToken = await DbContext.RefreshTokens.AsNoTracking().SingleAsync();
        var request = RequestNewTokenJsonBuilder.Build(previousToken);

        var response = await Post(REQUEST_URI, request);

        await AssertUnauthorized(response);
        await AssertTokenIsUnchanged(savedToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ShouldRotateUnexpiredToken_WithOrWithoutSurroundingWhitespace(bool hasSurroundingWhitespace)
    {
        var token = await AddRefreshToken();
        token.CreatedOn = DateTimeOffset.UtcNow.AddDays(-7).AddMinutes(5);
        await DbContext.SaveChangesAsync();
        var value = hasSurroundingWhitespace ? $" \t{token.Value}\r\n " : token.Value;

        var response = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(value));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tokens = await ReadTokens(response);
        var savedToken = await DbContext.RefreshTokens.AsNoTracking().SingleAsync();
        savedToken.Value.ShouldBe(tokens.RefreshToken);
        savedToken.Value.ShouldNotBe(token.Value);
        savedToken.CreatedOn.ShouldBeGreaterThan(token.CreatedOn);
    }

    [Theory]
    [ClassData(typeof(CultureInlineData))]
    public async Task ShouldReturnUnauthorized_WhenTokenIsExpired(string culture)
    {
        var token = await AddRefreshToken();
        token.CreatedOn = DateTimeOffset.UtcNow.AddDays(-7).AddMinutes(-5);
        await DbContext.SaveChangesAsync();

        var response = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(token.Value), culture: culture);

        await AssertUnauthorized(response, culture);
        await AssertTokenIsUnchanged(token);
    }

    [Theory]
    [InlineData("not-a-base64-token!")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public async Task ShouldReturnUnauthorized_WhenTokenIsInvalid(string value)
    {
        var token = await AddRefreshToken();

        var response = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(value));

        await AssertUnauthorized(response);
        await AssertTokenIsUnchanged(token);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var token = await AddRefreshToken();

        var response = await Post(REQUEST_URI, new { });

        await AssertUnauthorized(response);
        await AssertTokenIsUnchanged(token);
    }

    [Theory]
    [InlineData(null, "RefreshToken")]
    [InlineData(123, "$.refreshToken")]
    public async Task ShouldReturnBadRequest_WhenTokenIsNullOrNotAString(int? value, string errorKey)
    {
        var token = await AddRefreshToken();

        var response = await Post(REQUEST_URI, new { refreshToken = value });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var responseData = await ReadResponse(response);
        responseData.RootElement.GetProperty("status").GetInt32().ShouldBe(400);
        var errors = responseData.RootElement.GetProperty("errors").GetProperty(errorKey).EnumerateArray();
        errors.ShouldNotBeEmpty();
        errors.ShouldAllBe(error => !string.IsNullOrWhiteSpace(error.GetString()));
        await AssertTokenIsUnchanged(token);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenTokenIsInactive()
    {
        var token = await AddRefreshToken();
        token.Active = false;
        await DbContext.SaveChangesAsync();

        var response = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(token.Value));

        await AssertUnauthorized(response);
        await AssertTokenIsUnchanged(token);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenUserIsInactive()
    {
        var token = await AddRefreshToken();
        await DbContext.Users.Where(user => user.Id == _user1.GetId())
            .ExecuteUpdateAsync(setter => setter.SetProperty(user => user.Active, false));

        var response = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(token.Value));

        await AssertUnauthorized(response);
        await AssertTokenIsUnchanged(token);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenUserAndTokenWereDeletedFromDatabase()
    {
        var (user, _) = UserBuilder.Build();
        var token = RefreshTokenBuilder.Build(user);
        await DbContext.Users.AddAsync(user);
        await DbContext.RefreshTokens.AddAsync(token);
        await DbContext.SaveChangesAsync();
        await DbContext.RefreshTokens.Where(savedToken => savedToken.UserId == user.Id).ExecuteDeleteAsync();
        await DbContext.Users.Where(savedUser => savedUser.Id == user.Id).ExecuteDeleteAsync();

        var response = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(token.Value));

        await AssertUnauthorized(response);
        var tokensExist = await DbContext.RefreshTokens.AnyAsync();
        tokensExist.ShouldBeFalse();
        var userExists = await DbContext.Users.AnyAsync(savedUser => savedUser.Id == user.Id);
        userExists.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldReplaceOnlyTokensFromTheSameUser_WhenRotatingToken()
    {
        var token = await AddRefreshToken();
        var user = await DbContext.Users.SingleAsync(user => user.Id == _user1.GetId());
        var anotherToken = RefreshTokenBuilder.Build(user);
        var (otherUser, _) = UserBuilder.Build();
        var otherUserToken = RefreshTokenBuilder.Build(otherUser);
        await DbContext.Users.AddAsync(otherUser);
        await DbContext.RefreshTokens.AddRangeAsync(anotherToken, otherUserToken);
        await DbContext.SaveChangesAsync();

        var response = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(token.Value));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var responseTokens = await ReadTokens(response);
        var tokens = await DbContext.RefreshTokens.AsNoTracking().ToListAsync();
        tokens.Count.ShouldBe(2);
        var savedToken = tokens.Single(savedToken => savedToken.UserId == _user1.GetId());
        savedToken.Value.ShouldBe(responseTokens.RefreshToken);
        savedToken.Id.ShouldNotBe(token.Id);
        savedToken.Id.ShouldNotBe(anotherToken.Id);
        var preservedToken = tokens.Single(savedToken => savedToken.UserId == otherUser.Id);
        preservedToken.Id.ShouldBe(otherUserToken.Id);
        preservedToken.Value.ShouldBe(otherUserToken.Value);
        preservedToken.CreatedOn.ShouldBe(otherUserToken.CreatedOn);
        preservedToken.Active.ShouldBeTrue();
    }

    private async Task<RefreshToken> AddRefreshToken()
    {
        var user = await DbContext.Users.SingleAsync(user => user.Id == _user1.GetId());
        var token = RefreshTokenBuilder.Build(user);
        await DbContext.RefreshTokens.AddAsync(token);
        await DbContext.SaveChangesAsync();
        return token;
    }

    private async Task AssertTokenIsUnchanged(RefreshToken token)
    {
        var savedToken = await DbContext.RefreshTokens.AsNoTracking().SingleAsync();
        savedToken.Id.ShouldBe(token.Id);
        savedToken.Value.ShouldBe(token.Value);
        savedToken.UserId.ShouldBe(token.UserId);
        savedToken.CreatedOn.ShouldBe(token.CreatedOn);
        savedToken.Active.ShouldBe(token.Active);
    }

    private async Task<(string AccessToken, string RefreshToken)> RenewToken(string value)
    {
        var response = await Post(REQUEST_URI, RequestNewTokenJsonBuilder.Build(value));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await ReadTokens(response);
    }

    private static async Task<JsonDocument> ReadResponse(HttpResponseMessage response)
    {
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(responseBody);
    }

    private static async Task<(string AccessToken, string RefreshToken)> ReadTokens(HttpResponseMessage response)
    {
        using var responseData = await ReadResponse(response);
        return (
            responseData.RootElement.GetProperty("accessToken").GetString()!,
            responseData.RootElement.GetProperty("refreshToken").GetString()!);
    }

    private static async Task AssertUnauthorized(HttpResponseMessage response, string culture = "en-US")
    {
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using var responseData = await ReadResponse(response);
        var expectedMessage = ResourceMessagesException.ResourceManager
            .GetString("EXPIRED_REFRESH_TOKEN", new CultureInfo(culture));
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        errors.Count().ShouldBe(1);
        errors.Single().GetString().ShouldBe(expectedMessage);
        responseData.RootElement.GetProperty("accessTokenExpired").GetBoolean().ShouldBeFalse();
    }
}
