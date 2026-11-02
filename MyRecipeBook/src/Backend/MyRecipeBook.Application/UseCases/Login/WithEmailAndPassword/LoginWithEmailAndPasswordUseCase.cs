using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.RefreshToken;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Security.PasswordHashing;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.Storage;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Login.WithEmailAndPassword;

public class LoginWithEmailAndPasswordUseCase : ILoginWithEmailAndPasswordUseCase
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokensGenerator _acessTokenGenerator;
    private readonly IStorageService _storageService;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenWriteOnlyRepository _refreshTokenWriteOnlyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginWithEmailAndPasswordUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IPasswordHasher passwordHasher,
        IAccessTokensGenerator acessTokenGenerator,
        IStorageService storageService,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenWriteOnlyRepository refreshTokenWriteOnlyRepository,
        IUnitOfWork unitOfWork)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _passwordHasher = passwordHasher;
        _acessTokenGenerator = acessTokenGenerator;
        _storageService = storageService;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenWriteOnlyRepository = refreshTokenWriteOnlyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseRegisteredUserJson> Execute(RequestLoginJson request)
    {
        var user = await _userReadOnlyRepository.GetByEmail(request.Email);
        if (user is null)
            throw new InvalidLoginException();

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.Password);
        if (isPasswordValid is false)
            throw new InvalidLoginException();

        var refreshToken = await GenerateAndSaveRefreshToken(user.Id);

        await _unitOfWork.Commit();

        return new ResponseRegisteredUserJson
        {
            Name = user.Name,
            ImageUrl = user.HasImage ? _storageService.GetProfilePictureUrl(user) : string.Empty,
            Tokens =
            {
                AccessToken = _acessTokenGenerator.Generate(user),
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
