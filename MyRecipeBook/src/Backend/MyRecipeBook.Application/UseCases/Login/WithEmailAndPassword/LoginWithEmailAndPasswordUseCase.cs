using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
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

    public LoginWithEmailAndPasswordUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IPasswordHasher passwordHasher,
        IAccessTokensGenerator acessTokenGenerator,
        IStorageService storageService)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _passwordHasher = passwordHasher;
        _acessTokenGenerator = acessTokenGenerator;
        _storageService = storageService;
    }

    public async Task<ResponseRegisteredUserJson> Execute(RequestLoginJson request)
    {
        var user = await _userReadOnlyRepository.GetByEmail(request.Email);   
        if (user is null)
            throw new InvalidLoginException();
         
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.Password);
        if (isPasswordValid is false)
            throw new InvalidLoginException();

        return new ResponseRegisteredUserJson
        {
            Name = user.Name,
            ImageUrl = _storageService.GetProfilePictureUrl(user),
            Tokens =
            {
                AccessToken = _acessTokenGenerator.Generate(user)
            }
        };
    }
}
