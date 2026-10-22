using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.RefreshToken;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Token.UseRefreshToken;

public class UseRefreshTokenUseCase : IUseRefreshTokenUseCase
{
    private const int ExpirationTimeInDays = 7;

    private readonly IRefreshTokenReadOnlyRepository _refreshTokenReadOnlyRepositoy;
    private readonly IRefreshTokenWriteOnlyRepository _refreshTokenWriteOnlyRepository;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IAccessTokensGenerator _accessTokensGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public UseRefreshTokenUseCase(
        IRefreshTokenReadOnlyRepository refreshTokenReadOnlyRepositoy,
        IRefreshTokenWriteOnlyRepository refreshTokenWriteOnlyRepository, 
        IUserReadOnlyRepository userReadOnlyRepository, 
        IAccessTokensGenerator accessTokensGenerator, 
        IRefreshTokenGenerator refreshTokenGenerator, 
        IUnitOfWork unitOfWork)
    {
        _refreshTokenReadOnlyRepositoy = refreshTokenReadOnlyRepositoy;
        _refreshTokenWriteOnlyRepository = refreshTokenWriteOnlyRepository;
        _userReadOnlyRepository = userReadOnlyRepository;
        _accessTokensGenerator = accessTokensGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseTokensJson> Execute(RequestNewTokenJson request)
    {
        var refreshToken = await _refreshTokenReadOnlyRepositoy.Get(request.RefreshToken);
        if (refreshToken is null)
            throw new RefreshTokenExpiredException();

        var isValid = refreshToken.CreatedOn.AddDays(ExpirationTimeInDays) >= DateTimeOffset.UtcNow;
        if(isValid is false)
            throw new RefreshTokenExpiredException();

        var user = await _userReadOnlyRepository.GetById(refreshToken.UserId);

        var newRefreshToken = new RefreshToken
        {
            Value = _refreshTokenGenerator.Generate(),
            UserId = refreshToken.UserId
        };

        await _refreshTokenWriteOnlyRepository.Replace(newRefreshToken);
        await _unitOfWork.Commit();

        return new ResponseTokensJson
        {
            AccessToken = _accessTokensGenerator.Generate(user!),
            RefreshToken = newRefreshToken.Value

        };
    }
}
 