using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
        var responseParsed = TryParseResponse<TResult>(responseText);

        return responseParsed;
    }

    static TResult TryParseResponse<TResult>(string responseText)
    {
        try
        {
            var responseParsed = JsonSerializer.Deserialize<TResult>(responseText);
            ArgumentNullException.ThrowIfNull(responseParsed, nameof(responseParsed));

            return responseParsed;
        }
        catch (Exception ex) when (ex is JsonException || ex is ArgumentNullException)
        {
            throw new ArgumentException($@"Error occured whilst attempting deserialisation
Attempt deserialise to type: {typeof(TResult)}
Exception type: {ex.GetType()}
Exception message: {ex.Message}
Serialised text: {responseText}");
        }
    }

    async Task<HttpRequestMessage> GetAuthenticatedRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var idToken = await authService.GetIdToken();

        request.Headers.Authorization = new AuthenticationHeaderValue(idToken);
        return request;
    }
}
