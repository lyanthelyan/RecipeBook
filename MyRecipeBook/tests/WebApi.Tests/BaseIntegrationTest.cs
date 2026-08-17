using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Infrastructure.DataAcess;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace WebApi.Tests;

public abstract class BaseIntegrationTest : IClassFixture<MyRecipeBookApplicationFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    private readonly HttpClient _httpClient;
    internal readonly MyRecipeBookDbContext DbContext;

    public BaseIntegrationTest(MyRecipeBookApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();

        _scope = factory.Services.CreateScope();

        DbContext = _scope.ServiceProvider.GetRequiredService<MyRecipeBookDbContext>();
    }

    protected async Task<HttpResponseMessage> Post(string requestUri, object request, string accessToken = "",string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(accessToken);
        return await _httpClient.PostAsJsonAsync(requestUri, request);
    }

    protected async Task<HttpResponseMessage> Get(string requestUri, string accessToken, string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(accessToken);
        return await _httpClient.GetAsync(requestUri);
    }

    protected async Task<HttpResponseMessage> Put(string requestUri, object request, string accessToken, string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(accessToken);
        return await _httpClient.PutAsJsonAsync(requestUri, request);
    }

    protected async Task<HttpResponseMessage> Delete(string requestUri, string accessToken, string culture = "en-US")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(accessToken);
        return await _httpClient.DeleteAsync(requestUri);
    }

    private void ChangeRequestCulture(string culture)
    {
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd(culture);
    }

    private void AuthorizeRequest(string accessToken)
    {
        if (accessToken.IsNotEmpty())
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    }

    public void Dispose()
    {
        _scope?.Dispose();
        DbContext?.Dispose();
    }
}
