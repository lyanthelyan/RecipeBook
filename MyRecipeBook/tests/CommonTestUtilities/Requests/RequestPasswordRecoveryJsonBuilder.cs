using MyRecipeBook.Communication.Requests;
using Bogus;
namespace CommonTestUtilities.Requests;

public class RequestPasswordRecoveryJsonBuilder
{
    public static RequestPasswordRecoveryJson Build()
    {
        return new Faker<RequestPasswordRecoveryJson>()
            .RuleFor(request => request.Email, f => f.Internet.Email());
    }
}
