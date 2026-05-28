using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.User.Register;

public class RegisterUserAccountUseCase
{
    public void Execute(RequestRegisterUserAccountJson request)
    {
        // Creates the validator responsible
        // for validating the request data
        var validator = new RegisterUserAccountValidator();

        // Executes the validation
        var result = validator.Validate(request);

        // Checks if the validation failed
        if (result.IsValid == false)
        {
            // Gets all validation error messages
            var errorMessages = result.Errors
                .Select(error => error.ErrorMessage)
                .ToList();

            // Throws a custom validation exception
            throw new ErrorOnValidationException(errorMessages);
        }
    }
}
