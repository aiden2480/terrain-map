using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainAuthService(HttpClient httpClient) : ITerrainAuthService
{
    const string CognitoUrl = "https://cognito-idp.ap-southeast-2.amazonaws.com";

    const string InitiateAuthHeaderValue = "AWSCognitoIdentityProviderService.InitiateAuth";

    const string ClientId = "6v98tbc09aqfvh52fml3usas3c";

    public async Task<LoginApiResponse> AttemptLoginWithCredentials(Branch branch, uint memberNumber, string password)
        => await MakeRequestToLoginApi(new
        {
            ClientId,
            AuthFlow = "USER_PASSWORD_AUTH",
            AuthParameters = new
            {
                USERNAME = $"{branch.ToString().ToLower()}-{memberNumber}",
                PASSWORD = password
            }
        });

    public async Task<LoginApiResponse> AttemptLoginWithRefreshToken(string refreshToken)
        => await MakeRequestToLoginApi(new
        {
            ClientId,
            AuthFlow = "REFRESH_TOKEN_AUTH",
            AuthParameters = new
            {
                REFRESH_TOKEN = refreshToken,
                DEVICE_KEY = (string)null!
            }
        });

    async Task<LoginApiResponse> MakeRequestToLoginApi<TRequestContent>(TRequestContent requestContent)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, CognitoUrl);

        request.Headers.Add("X-amz-target", InitiateAuthHeaderValue);
        request.Content = new StringContent(JsonSerializer.Serialize(requestContent));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-amz-json-1.1");

        var response = await httpClient.SendAsync(request);
        var respData = await response.Content.ReadAsStringAsync();

        return response.IsSuccessStatusCode
            ? ParseSuccessfulLogin(respData)
            : ParseUnsuccessfulLogin(respData);
    }

    #region Helpers

    static LoginApiResponse ParseSuccessfulLogin(string respData)
    {
        var result = JsonSerializer.Deserialize<SuccessfulLoginResponse>(respData);
        ArgumentNullException.ThrowIfNull(result, $"Could not deserialise successful login: {respData}");

        return new LoginApiResponse
        {
            AccessToken = result.AuthenticationResult.AccessToken,
            AccessTokenExpires = DateTime.Now.AddSeconds(result.AuthenticationResult.ExpiresIn),
            IdToken = result.AuthenticationResult.IdToken,
            RefreshToken = result.AuthenticationResult.RefreshToken,
        };
    }

    static LoginApiResponse ParseUnsuccessfulLogin(string respData)
    {
        var result = JsonSerializer.Deserialize<UnsuccessfulLoginResponse>(respData);
        ArgumentNullException.ThrowIfNull(result, $"Could not deserialise unsuccessful login: {respData}");

        return new LoginApiResponse
        {
            ErrorMessage = $"{result.ExceptionType} - {result.Message}"
        };
    }

    record SuccessfulLoginResponse(AuthenticationResult AuthenticationResult);

    record AuthenticationResult(
        string AccessToken,
        int ExpiresIn,
        string IdToken,
        string RefreshToken,
        string TokenType);

    record UnsuccessfulLoginResponse(
        [property: JsonPropertyName("__type")] string ExceptionType,
        [property: JsonPropertyName("message")] string Message);

    #endregion
}
