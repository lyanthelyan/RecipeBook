using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Security.PasswordHashing;

namespace MyRecipeBook.Application.UseCases.Login.WithEmailAndPassword;

public class LoginWithEmailAndPasswordUseCase : ILoginWithEmailAndPasswordUseCase
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginWithEmailAndPasswordUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IPasswordHasher passwordHasher)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResponseRegisteredUserJson> Execute(RequestLoginJson request)
    {
        var user  = await _userReadOnlyRepository.GetByEmail(request.Email);

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.Password);

        return new ResponseRegisteredUserJson
        {
            Name = user.Name
        };
    }
}
