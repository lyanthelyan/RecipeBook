using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Security;
using CommonTestUtilities.Storage;
using MyRecipeBook.Application.UseCases.Login.WithExternalProvider;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;
using Shouldly;
using System.Net;

namespace UseCases.Tests.User.Login.WithExternalProvider;

public class ExchangeExternalLoginCodeUseCaseTests
{

    [Theory]
    [InlineData(true, IStorageServiceBuilder.FakeUrl)]
    [InlineData(false, "")]
    public async Task Success(bool hasImage, string expectedUrl)
    {
        var (user, _) = UserBuilder.Build();
        user.HasImage = hasImage;

        var verificationCode = VerificationCodeBuilder.Build(user);
        verificationCode.Type = VerificationCodeType.ExternalLoginExchange;

        var request = new RequestExternalLoginJson
        {
            Code = verificationCode.Code
        };

        var useCase = CreateUseCase(verificationCode, user);

        var result = await useCase.Execute(request);

        result.ShouldNotBeNull();
        result.Name.ShouldBe(user.Name);
        result.Tokens.ShouldNotBeNull();
        result.Tokens.AccessToken.ShouldNotBeNullOrEmpty();
        result.ImageUrl.ShouldBe(expectedUrl);
    }

    [Fact]
    public async Task ShouldThrowException_WhenCodeDoesNotExist()
    {
        var request = new RequestExternalLoginJson
        {
            Code = "non-existent-code"
        };
        var useCase = CreateUseCase();

        var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);

        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages =>
        {
            errorMessages.Count.ShouldBe(1);
            errorMessages.ShouldContain(ResourceMessagesException.VERIFICATION_CODE_INVALID);
        });


    }
    [Fact]
    public async Task ShouldThrowException_WhenCodeIsExpired()
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);
        verificationCode.Type = VerificationCodeType.ExternalLoginExchange;
        verificationCode.CreatedOn = DateTimeOffset.UtcNow.AddSeconds(-31);

        var request = new RequestExternalLoginJson
        {
            Code = verificationCode.Code
        };
        var useCase = CreateUseCase(verificationCode, user);
        
        var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);

        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages =>
        {
            errorMessages.Count.ShouldBe(1);
            errorMessages.ShouldContain(ResourceMessagesException.VERIFICATION_CODE_INVALID);
        });


    }


    [Fact]
    public async Task ShouldThrowException_WhenCodeWasAlreadyConsumed()
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);
        verificationCode.Type = VerificationCodeType.ExternalLoginExchange;

        var request = new RequestExternalLoginJson
        {
            Code = verificationCode.Code
        };
        var useCase = CreateUseCase(verificationCode, user, deletedRows: 0);

        var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages =>
        {
            errorMessages.Count.ShouldBe(1);
            errorMessages.ShouldContain(ResourceMessagesException.VERIFICATION_CODE_INVALID);
        });
    }

    private static ExchangeExternalLoginCodeUseCase CreateUseCase(
    VerificationCode? verificationCode = null,
    MyRecipeBook.Domain.Entities.User? user = null,
    int deletedRows = 1)
    {
        var verificationCodeReadOnlyRepositoryBuilder =
            new IVerificationCodeReadOnlyRepositoryBuilder();

        if (verificationCode is not null)
            verificationCodeReadOnlyRepositoryBuilder
                .GetExternalLoginCode(verificationCode);

        var userReadOnlyRepositoryBuilder =
            new IUserReadOnlyRepositoryBuilder();

        if (user is not null)
            userReadOnlyRepositoryBuilder.GetById(user);

        var verificationCodeWriteOnlyRepository =
            IVerificationCodeWriteOnlyRepositoryBuilder.Build(deletedRows);

        var accessTokenGenerator =
            IAcessTokenGeneratorBuilder.Build();

        var storageService =
            IStorageServiceBuilder.Build();

        var refreshTokenGenerator = IRefreshTokenGeneratorBuilder.Build();
        var refreshTokenWriteOnlyRepository = IRefreshTokenWriteOnlyRepositoryBuilder.Build();
        var unitOfWork = IUnitOfWorkBuilder.Build();

        return new ExchangeExternalLoginCodeUseCase(
            verificationCodeReadOnlyRepositoryBuilder.Build(),
            verificationCodeWriteOnlyRepository,
            userReadOnlyRepositoryBuilder.Build(),
            accessTokenGenerator,
            storageService,
            refreshTokenGenerator,
            refreshTokenWriteOnlyRepository,
            unitOfWork);
    }
}
