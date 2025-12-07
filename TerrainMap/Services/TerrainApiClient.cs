using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainApiClient(HttpClient httpClient, IStorageService storageService) : ITerrainApiClient
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    public async Task<TResult> SendGet<TResult>(string url)
    {
        var request = await GetAuthenticatedRequest(url, HttpMethod.Get);
        var response = await SendAuthenticatedRequest<TResult>(request);

        return response;
    }

    public async Task<TResult> SendPost<TContent, TResult>(string url, TContent content)
    {
        var request = await GetAuthenticatedRequest(url, HttpMethod.Post);
        request.Content = new StringContent(JsonSerializer.Serialize(content));
        var response = await SendAuthenticatedRequest<TResult>(request);

        return response;
    }

    public async Task SendPostVoid<TContent>(string url, TContent content)
    {
        var request = await GetAuthenticatedRequest(url, HttpMethod.Post);

        request.Content = new StringContent(JsonSerializer.Serialize(content));
        await httpClient.SendAsync(request);
    }

    async Task<HttpRequestMessage> GetAuthenticatedRequest(string url, HttpMethod method)
    {
        var request = new HttpRequestMessage(method, url);
        var idToken = await storageService.GetIdToken();

        request.Headers.Authorization = new AuthenticationHeaderValue(idToken);
        return request;
    }

    async Task<TResult> SendAuthenticatedRequest<TResult>(HttpRequestMessage request)
    {
        var response = await httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();
        var responseParsed = TryParseResponse<TResult>(responseText);

        return responseParsed;
    }

    static TResult TryParseResponse<TResult>(string responseText)
    {
        try
        {
            var responseParsed = JsonSerializer.Deserialize<TResult>(responseText, JsonOptions);
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
}
