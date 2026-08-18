using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Repositories.VerificationCode;
using MyRecipeBook.Domain.Services.Email;
using System.Security.Cryptography;

namespace MyRecipeBook.Application.UseCases.PasswordRecovery.RequestCode;

public class RequestPasswordRecoveryCodeUseCase : IRequestPasswordRecoveryCodeUseCase
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IVerificationCodeWriteOnlyRepository _verificationCodeWriteOnlyRepository;
    private readonly IUnitOfWork _unitOfWork;

    //private readonly IEmailService _emailService;

    public RequestPasswordRecoveryCodeUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IVerificationCodeWriteOnlyRepository verificationCodeWriteOnlyRepository,
        IUnitOfWork unitOfWork
        //IEmailService emailService
        )
       
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _verificationCodeWriteOnlyRepository = verificationCodeWriteOnlyRepository;
        _unitOfWork = unitOfWork;
        //_emailService = emailService
    }

    public async Task Execute(RequestPasswordRecoveryJson request)
    {
        var user = await _userReadOnlyRepository.GetByEmail(request.Email);
        if (user is null)
            return;

        var code = RandomNumberGenerator.GetInt32(1, 1_000_000);

        var verificationCode = new VerificationCode
        {
            Code = code.ToString("D6"),
            Type = VerificationCodeType.PasswordRecovery,
            UserId = user.Id
        };
        

        await _verificationCodeWriteOnlyRepository.Replace(verificationCode);

        await _unitOfWork.Commit();
        //TODO: Send email with the code
        //await _emailService.SendPasswordRecoveryCode(
        //     user.Email,
        //     verificationCode.Code);
    }
}
