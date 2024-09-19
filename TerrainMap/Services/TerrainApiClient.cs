using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainApiClient(HttpClient httpClient, ILocalAuthService authService) : ITerrainApiClient
{
    public async Task<TResult> SendAuthenticatedRequest<TResult>(string url)
    {
        var request = await GetAuthenticatedRequest(url);
        var response = await httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();
        var responseParsed = JsonSerializer.Deserialize<TResult>(responseText);

        Debug.Assert(responseParsed is not null);
        return responseParsed;
    }

    async Task<HttpRequestMessage> GetAuthenticatedRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var idToken = await authService.GetIdToken();

        request.Headers.Authorization = new AuthenticationHeaderValue(idToken);
        return request;
    }
}
