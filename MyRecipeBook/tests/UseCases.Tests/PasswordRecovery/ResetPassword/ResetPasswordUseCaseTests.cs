using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using MyRecipeBook.Application.UseCases.PasswordRecovery.ResetPassword;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;
using Shouldly;
using System.Net;

namespace UseCases.Tests.PasswordRecovery.ResetPassword;

public class ResetPasswordUseCaseTests
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);

        var request = RequestResetPasswordJsonBuilder.Build();
        request.Email = user.Email;
        request.Code = verificationCode.Code;

        var useCase = CreateUseCase(user, verificationCode);

        await useCase.Execute(request).ShouldNotThrowAsync();
       }

    [Fact]
    public async Task ShouldThrowException_WhenUserDoesNotExist()
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);

        var request = RequestResetPasswordJsonBuilder.Build();
        request.Code = verificationCode.Code;

        var useCase = CreateUseCase(user, verificationCode);
        var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourceMessagesException.VERIFICATION_CODE_INVALID);
        });

    }
    [Fact]
    public async Task ShouldThrowException_WhenCodeDoesNotExist()
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);

        var request = RequestResetPasswordJsonBuilder.Build();
        request.Email = user.Email;

        var useCase = CreateUseCase(user, verificationCode);
        var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourceMessagesException.VERIFICATION_CODE_INVALID);
        });

    }

    [Fact]
    public async Task ShouldThrowException_WhenCodeIsExpired()
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);
        verificationCode.CreatedOn = DateTimeOffset.UtcNow.AddMinutes(-11);
        
        var request = RequestResetPasswordJsonBuilder.Build();
        request.Email = user.Email;
        request.Code = verificationCode.Code;

        var useCase = CreateUseCase(user, verificationCode);
        var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourceMessagesException.VERIFICATION_CODE_INVALID);
        });

    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task ShouldThrowException_WhenNewPasswordIsInvalid(int passwordLenght)
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);

        var request = RequestResetPasswordJsonBuilder.Build(passwordLenght);
        request.Email = user.Email;
        request.Code = verificationCode.Code;

        var useCase = CreateUseCase(user, verificationCode);

        var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourceMessagesException.VALIDATION_PASSWORD_MIN_LENGTH);
        });

    }

    [Fact]
    public async Task ShouldThrowException_WhenCodeWasAlreadyConsumed()
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);
        var verificationCodeWriteOnlyRepository =
            IVerificationCodeWriteOnlyRepositoryBuilder.Build(0);
        
        var request = RequestResetPasswordJsonBuilder.Build();
        request.Email = user.Email;
        request.Code = verificationCode.Code;

        var useCase = CreateUseCase(user, verificationCode, deletedRows: 0);

        var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();

        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldContain(ResourceMessagesException.VERIFICATION_CODE_INVALID);
    }

    [Fact]
    public async Task ShouldThrowException_WhenPasswordUpdateFails()
    {
        var (user, _) = UserBuilder.Build();
        var verificationCode = VerificationCodeBuilder.Build(user);

        var request = RequestResetPasswordJsonBuilder.Build();
        request.Email = user.Email;
        request.Code = verificationCode.Code;

        var userUpdateRepository = IUserUpdateOnlyRepositoryBuilder.Build(0);

        var useCase = CreateUseCase(
            user,
            verificationCode, userUpdateOnlyRepository: userUpdateRepository);

        var exception = await useCase.Execute(request)
            .ShouldThrowAsync<InvalidOperationException>();

        exception.Message.ShouldBe(
            ResourceMessagesException.UNABLE_TO_UPDATE_PASSWORD);
    }

    private static ResetPasswordUseCase CreateUseCase(
    MyRecipeBook.Domain.Entities.User user,
    VerificationCode verificationCode,
    int deletedRows = 1,
    IUserUpdateOnlyRepository? userUpdateOnlyRepository = null)
    {
        var userReadOnlyRepository = new IUserReadOnlyRepositoryBuilder()
            .GetByEmail(user)
            .Build();

        var verificationCodeReadOnlyRepository = new IVerificationCodeReadOnlyRepositoryBuilder()
            .Get(verificationCode)
            .Build();

        var verificationCodeWriteOnlyRepository =
            IVerificationCodeWriteOnlyRepositoryBuilder.Build(deletedRows);

        userUpdateOnlyRepository ??= IUserUpdateOnlyRepositoryBuilder.Build();

        var passwordHasher = new IPasswordHasherBuilder().Build();

        var unitOfWork = IUnitOfWorkBuilder.Build();

        return new ResetPasswordUseCase(
            userReadOnlyRepository,
            verificationCodeReadOnlyRepository,
            verificationCodeWriteOnlyRepository,
            userUpdateOnlyRepository,
            passwordHasher,
            unitOfWork);
    }
}
