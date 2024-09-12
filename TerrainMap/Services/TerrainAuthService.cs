using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public async Task<LoginApiResponse> AttemptLoginWithCredentials(Branch branch, int memberNumber, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, CognitoUrl);
        var body = new
        {
            ClientId = "6v98tbc09aqfvh52fml3usas3c",
            AuthFlow = "USER_PASSWORD_AUTH",
            AuthParameters = new
            {
                USERNAME = $"{branch.ToString().ToLower()}-{memberNumber}",
                PASSWORD = password
            }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(body));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-amz-json-1.1");
        request.Headers.Add("X-amz-target", "AWSCognitoIdentityProviderService.InitiateAuth");

        var response = await httpClient.SendAsync(request);
        var respData = await response.Content.ReadAsStringAsync();

        return response.IsSuccessStatusCode
            ? ParseSuccessfulLogin(respData)
            : ParseUnsuccessfulLogin(respData);
    }

    #region AttemptLoginWithCredentials Helpers

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
