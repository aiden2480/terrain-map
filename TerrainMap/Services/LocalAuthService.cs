using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;
using WebExtensions.Net;

namespace TerrainMap.Services;

public class LocalAuthService(ITerrainAuthService terrainAuthService, IWebExtensionsApi webExtensions) : ILocalAuthService
{
    // The refresh token never expires - I assume unless the user
    // changes their password, but we don't care about that.
    // For the user to be considered authenticated, we must have
    // access to the refresh token. We can then cache the access
    // token or retrieve a new one if need be. This way we never
    // need to store the username & password

    public async Task<bool> IsAuthenticated()
        => (await GetStorageTokens(skipReauthentication: true)).RefreshToken is not null;

    public async Task<string> GetAccessToken()
        => (await GetStorageTokens()).AccessToken!;

    public async Task<string> GetIdToken()
        => (await GetStorageTokens()).IdToken!;

    public async Task UpdateFromLoginApiResponse(LoginApiResponse loginApiResponse)
    {
        ArgumentNullException.ThrowIfNull(loginApiResponse.RefreshToken, "Attempted to upadte with null refresh token");

        var storageTokens = new StorageTokens(
            loginApiResponse.AccessToken,
            loginApiResponse.RefreshToken,
            loginApiResponse.IdToken,
            loginApiResponse.AccessTokenExpires);

        await webExtensions.Storage.Sync.Set(storageTokens);
    }

    async Task<StorageTokens> GetStorageTokens(bool skipReauthentication = false)
    {
        // Deserialise whatever is in the chrome storage
        var storageResult = await webExtensions.Storage.Sync.Get();
        var storageTokens = storageResult.Deserialize<StorageTokens>();
        ArgumentNullException.ThrowIfNull(storageTokens, nameof(storageTokens));

        // Check if the access token has expired, update if so
        if (NeedToReauthenticateWithRefreshToken(storageTokens) && !skipReauthentication)
        {
            // If we don't have a refresh token, return empty storage tokens
            if (storageTokens.RefreshToken is null)
            {
                return storageTokens;
            }

            // Update tokens & write back to storage
            storageTokens = await GenerateNewTokens(storageTokens.RefreshToken);
            await webExtensions.Storage.Sync.Set(storageTokens);
        }

        return storageTokens;
    }

    static bool NeedToReauthenticateWithRefreshToken(StorageTokens storageTokens)
        => storageTokens.IdToken is null
        || storageTokens.AccessToken is null
        || storageTokens.AccessTokenExpires is null
        || storageTokens.AccessTokenExpires <= DateTime.Now;

    async Task<StorageTokens> GenerateNewTokens(string refreshToken)
    {
        var loginApiResponse = await terrainAuthService.AttemptLoginWithRefreshToken(refreshToken);

        // The response for the refresh token auth doesn't
        // contain an auth token so we must sub it out with
        // our current one
        return new StorageTokens(
            loginApiResponse.AccessToken,
            loginApiResponse.RefreshToken ?? refreshToken,
            loginApiResponse.IdToken,
            loginApiResponse.AccessTokenExpires);
    }

    record StorageTokens(
        [property: JsonPropertyName("accessToken")] string? AccessToken,
        [property: JsonPropertyName("refreshToken")] string? RefreshToken,
        [property: JsonPropertyName("idToken")] string? IdToken,
        [property: JsonPropertyName("accessTokenExpires")] DateTime? AccessTokenExpires);
}
