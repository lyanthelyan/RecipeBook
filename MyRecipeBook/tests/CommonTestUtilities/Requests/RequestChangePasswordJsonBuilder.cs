using Bogus;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestChangePasswordJsonBuilder
{
    public static RequestChangePasswordJson Build(int newPasswordLength = 10) 
    {
        return new Faker<RequestChangePasswordJson>()
            .RuleFor(request => request.CurrentPassword, faker => faker.Internet.Password())
            .RuleFor(request => request.NewPassword, faker => faker.Internet.Password(length: newPasswordLength));
    }
    
}
