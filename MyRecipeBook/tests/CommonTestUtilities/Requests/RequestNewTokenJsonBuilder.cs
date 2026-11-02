using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestNewTokenJsonBuilder
{
    public static RequestNewTokenJson Build(string refreshToken)
    {
        return new RequestNewTokenJson
        {
            RefreshToken = refreshToken
        };
    }
}
