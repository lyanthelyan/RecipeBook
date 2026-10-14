using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Repositories.VerificationCode;
using MyRecipeBook.Domain.Security.PasswordHashing;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.PasswordRecovery.ResetPassword;

public class ResetPasswordUseCase : IResetPasswordUseCase
{
    private const int ExpirationTimeInMinutes = 10;

    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IVerificationCodeReadOnlyRepository _verificationCodeReadOnlyRepository;
    private readonly IVerificationCodeWriteOnlyRepository _verificationCodeWriteOnlyRepository;
    private readonly IUserUpdateOnlyRepository _userUpdateOnlyRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IVerificationCodeReadOnlyRepository verificationCodeReadOnlyRepository, 
        IVerificationCodeWriteOnlyRepository verificationCodeWriteOnlyRepository, 
        IUserUpdateOnlyRepository userUpdateOnlyRepository, 
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _verificationCodeReadOnlyRepository = verificationCodeReadOnlyRepository;
        _verificationCodeWriteOnlyRepository = verificationCodeWriteOnlyRepository;
        _userUpdateOnlyRepository = userUpdateOnlyRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task Execute(RequestResetPasswordJson request)
    {
        var user = await _userReadOnlyRepository.GetByEmail(request.Email);
        if (user is null)
            throw new ErrorOnValidationException([ResourceMessagesException.VERIFICATION_CODE_INVALID]);

        var verificationCode = await _verificationCodeReadOnlyRepository.Get(user.Id, request.Code, VerificationCodeType.PasswordRecovery);
        if(verificationCode is null)
            throw new ErrorOnValidationException([ResourceMessagesException.VERIFICATION_CODE_INVALID]);

        ValidateAndThrowOnFailures(request, verificationCode);

        var hashedPassword = _passwordHasher.HashPassword(request.NewPassword);

        //Atomic operation
        await _unitOfWork.ExecuteInTransaction(async () => 
        {
            var deletedRows = await _verificationCodeWriteOnlyRepository.Delete(verificationCode);
            if (deletedRows != 1)
            {
                throw new ErrorOnValidationException([ResourceMessagesException.VERIFICATION_CODE_INVALID]);
            }
            
            var updatedRows = await _userUpdateOnlyRepository.UpdatePassword(verificationCode.UserId, hashedPassword);
            if (updatedRows != 1)
            {
                throw new InvalidOperationException(ResourceMessagesException.UNABLE_TO_UPDATE_PASSWORD);
            }
        });
    }

    private void ValidateAndThrowOnFailures(RequestResetPasswordJson request, VerificationCode verificationCode)
    {
        var isCodeValid = verificationCode.CreatedOn.AddMinutes(ExpirationTimeInMinutes) >= DateTimeOffset.UtcNow;
        if (isCodeValid is false)
            throw new ErrorOnValidationException([ResourceMessagesException.VERIFICATION_CODE_INVALID]);
        var result = new ResetPasswordValidator().Validate(request);

        if (result.IsValid is false)
            throw new ErrorOnValidationException(result.Errors.Select(error => error.ErrorMessage).ToList());

    }
}
 