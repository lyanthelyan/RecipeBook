using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Repositories.VerificationCode;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.Storage;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Login.WithExternalProvider;

public class ExchangeExternalLoginCodeUseCase : IExchangeExternalLoginCodeUseCase
{
    private const int ExpirationTimeInSeconds = 30;

    private readonly IVerificationCodeReadOnlyRepository _verificationCodeReadOnlyRepository;
    private readonly IVerificationCodeWriteOnlyRepository _verificationCodeWriteOnlyRepository;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IAccessTokensGenerator _accessTokensGenerator;
    private readonly IStorageService _storageService;

    public ExchangeExternalLoginCodeUseCase(
        IVerificationCodeReadOnlyRepository verificationCodeReadOnlyRepository, 
        IVerificationCodeWriteOnlyRepository verificationCodeWriteOnlyRepository, 
        IUserReadOnlyRepository userReadOnlyRepository, 
        IAccessTokensGenerator accessTokensGenerator, 
        IStorageService storageService)
    {
        _verificationCodeReadOnlyRepository = verificationCodeReadOnlyRepository;
        _verificationCodeWriteOnlyRepository = verificationCodeWriteOnlyRepository;
        _userReadOnlyRepository = userReadOnlyRepository;
        _accessTokensGenerator = accessTokensGenerator;
        _storageService = storageService;
    }

    public async Task<ResponseRegisteredUserJson> Execute(RequestExternalLoginJson request)
    {
        var verificationCode = await _verificationCodeReadOnlyRepository.GetExternalLoginCode(request.Code);
        if (verificationCode is null)
            throw new ErrorOnValidationException([ResourceMessagesException.VERIFICATION_CODE_INVALID]);

        var isCodeValid = verificationCode.CreatedOn.AddSeconds(ExpirationTimeInSeconds) >= DateTimeOffset.UtcNow;
        if (isCodeValid == false)
            throw new ErrorOnValidationException([ResourceMessagesException.VERIFICATION_CODE_INVALID]);

        await _verificationCodeWriteOnlyRepository.Delete(verificationCode);

        var user = await _userReadOnlyRepository.GetById(verificationCode.UserId);

        return new ResponseRegisteredUserJson
        {
            Name = user!.Name,
            ImageUrl = user.HasImage ? _storageService.GetProfilePictureUrl(user) : string.Empty,
            Tokens = new ResponseTokensJson
            {
                AccessToken = _accessTokensGenerator.Generate(user)
            }
        };

    }
}
