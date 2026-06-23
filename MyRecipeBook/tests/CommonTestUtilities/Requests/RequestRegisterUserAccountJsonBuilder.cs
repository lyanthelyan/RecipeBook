using Bogus;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Rquests;

public class RequestRegisterUserAccountJsonBuilder
{
    public static RequestRegisterUserAccountJson Build()
    {
        return new Faker<RequestRegisterUserAccountJson>()
            .RuleFor(request => request.Name, faker => faker.Person.FirstName)
            .RuleFor(request => request.Email, (faker, user)=> faker.Internet.Email(user.Name))
            .RuleFor(request => request.Password, faker=> faker.Internet.Password());
    }
}