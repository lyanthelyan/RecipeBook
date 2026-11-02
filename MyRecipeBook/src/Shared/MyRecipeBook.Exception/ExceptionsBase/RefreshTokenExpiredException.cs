using System.Net;

namespace MyRecipeBook.Exception.ExceptionsBase;

public class RefreshTokenExpiredException : MyRecipeBookException
{
    public override List<string> GetErrorMessages()
    {
        return [ResourceMessagesException.EXPIRED_REFRESH_TOKEN];
    }

    public override HttpStatusCode GetStatusCode()
    {
        return HttpStatusCode.Unauthorized;
    }
}
