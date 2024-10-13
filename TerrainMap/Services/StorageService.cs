using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;
using WebExtensions.Net;

namespace TerrainMap.Services;

public class StorageService(ITerrainAuthService terrainAuthService, IWebExtensionsApi webExtensions) : IStorageService
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
        ArgumentNullException.ThrowIfNull(loginApiResponse.RefreshToken);

        var model = new StorageModel
        {
            AccessToken = loginApiResponse.AccessToken,
            RefreshToken = loginApiResponse.RefreshToken,
            IdToken = loginApiResponse.IdToken,
            AccessTokenExpires = loginApiResponse.AccessTokenExpires,
        };

        await UpdateModel(model);
    }

    public async Task<IEnumerable<Profile>?> GetProfilesFromStorage()
    {
        var model = await GetModel();

        return (model.Profiles is not null && model.ProfilesExpire is not null && model.ProfilesExpire > DateTime.Now)
            ? model.Profiles
            : null;
    }

    public async Task SetProfiles(IEnumerable<Profile> profiles)
    {
        var model = new StorageModel
        {
            Profiles = profiles,
            ProfilesExpire = DateTime.Now.AddDays(7),
        };

        await UpdateModel(model);
    }

    async Task<StorageModel> GetStorageTokens(bool skipReauthentication = false)
    {
        var model = await GetModel();

        // Check if the access token has expired, update if so
        if (NeedToReauthenticateWithRefreshToken(model) && !skipReauthentication)
        {
            // If we don't have a refresh token, return empty storage tokens
            if (model.RefreshToken is null)
            {
                await ClearModel();
                throw new ArgumentNullException("Trying to generate new tokens but we don't have the refresh token");
            }

            // Update tokens & write back to storage
            model = await GenerateNewTokens(model.RefreshToken);
            await UpdateModel(model);
        }

        return model;
    }

    static bool NeedToReauthenticateWithRefreshToken(StorageModel model)
        => model.IdToken is null
        || model.AccessToken is null
        || model.AccessTokenExpires is null
        || model.AccessTokenExpires <= DateTime.Now;

    async Task<StorageModel> GenerateNewTokens(string refreshToken)
    {
        var loginApiResponse = await terrainAuthService.AttemptLoginWithRefreshToken(refreshToken);

        // The response for the refresh token auth doesn't
        // contain an auth token so we must sub it out with
        // our current one
        return new StorageModel
        {
            AccessToken = loginApiResponse.AccessToken,
            RefreshToken = loginApiResponse.RefreshToken ?? refreshToken,
            IdToken = loginApiResponse.IdToken,
            AccessTokenExpires = loginApiResponse.AccessTokenExpires,
        };
    }

    async Task<StorageModel> GetModel()
    {
        var storageResult = await webExtensions.Storage.Sync.Get();
        var model = storageResult.Deserialize<StorageModel>();
        ArgumentNullException.ThrowIfNull(model);

        return model;
    }

    // Overwrites all properties not set
    // async Task SetModel(StorageModel model)
    //     => await webExtensions.Storage.Sync.Set(model);
    
    async Task UpdateModel(StorageModel model)
        => await webExtensions.Storage.Sync.Set(model
            .GetType()
            .GetProperties()
            .Where(p => p.GetValue(model) is not null)
            .ToDictionary(p => p.Name, p => p.GetValue(model)));
    
    async Task ClearModel()
        => await webExtensions.Storage.Sync.Clear();

    class StorageModel
    {
        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        public string? IdToken { get; set; }

        public DateTime? AccessTokenExpires { get; set; }

        public IEnumerable<Profile>? Profiles { get; set; }

        public DateTime? ProfilesExpire { get; set; }
    }
}
