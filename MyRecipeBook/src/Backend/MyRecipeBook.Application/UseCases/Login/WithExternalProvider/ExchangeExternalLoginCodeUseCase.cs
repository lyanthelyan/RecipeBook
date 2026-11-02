using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.RefreshToken;
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
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenWriteOnlyRepository _refreshTokenWriteOnlyRepository;
    private readonly IUnitOfWork _unitOfWork;


    public ExchangeExternalLoginCodeUseCase(
        IVerificationCodeReadOnlyRepository verificationCodeReadOnlyRepository, 
        IVerificationCodeWriteOnlyRepository verificationCodeWriteOnlyRepository, 
        IUserReadOnlyRepository userReadOnlyRepository, 
        IAccessTokensGenerator accessTokensGenerator, 
        IStorageService storageService,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenWriteOnlyRepository refreshTokenWriteOnlyRepository,
        IUnitOfWork unitOfWork)
    {
        _verificationCodeReadOnlyRepository = verificationCodeReadOnlyRepository;
        _verificationCodeWriteOnlyRepository = verificationCodeWriteOnlyRepository;
        _userReadOnlyRepository = userReadOnlyRepository;
        _accessTokensGenerator = accessTokensGenerator;
        _storageService = storageService;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenWriteOnlyRepository = refreshTokenWriteOnlyRepository;
        _unitOfWork = unitOfWork;

    }

    public async Task<ResponseRegisteredUserJson> Execute(RequestExternalLoginJson request)
    {
        var verificationCode = await _verificationCodeReadOnlyRepository.GetExternalLoginCode(request.Code);
        if (verificationCode is null)
            throw new ErrorOnValidationException([ResourceMessagesException.VERIFICATION_CODE_INVALID]);

        var isCodeValid = verificationCode.CreatedOn.AddSeconds(ExpirationTimeInSeconds) >= DateTimeOffset.UtcNow;
        if (isCodeValid == false)
            throw new ErrorOnValidationException([ResourceMessagesException.VERIFICATION_CODE_INVALID]);

        var affectedRows = await _verificationCodeWriteOnlyRepository.Delete(verificationCode);
        // Race condition fix
        if (affectedRows != 1)
            throw new ErrorOnValidationException(
                [ResourceMessagesException.VERIFICATION_CODE_INVALID]);
        
        var user = await _userReadOnlyRepository.GetById(verificationCode.UserId);

        var refreshToken = await GenerateAndSaveRefreshToken(user!.Id);

        await _unitOfWork.Commit();

        return new ResponseRegisteredUserJson
        {
            Name = user!.Name,
            ImageUrl = user.HasImage ? _storageService.GetProfilePictureUrl(user) : string.Empty,
            Tokens = new ResponseTokensJson
            {
                AccessToken = _accessTokensGenerator.Generate(user),
                RefreshToken = refreshToken
            }
        };

    }
    private async Task<string> GenerateAndSaveRefreshToken(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Value = _refreshTokenGenerator.Generate(),
            UserId = userId
        };

        await _refreshTokenWriteOnlyRepository.Replace(refreshToken);

        return refreshToken.Value;
    }
}
