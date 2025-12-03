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
    public async Task<bool> IsAuthenticated()
    {
        var modelFromStorage = await GetModelFromStorage();

        // If there's nothing in the storage already, we are definitely not authenticated
        if (modelFromStorage.RefreshToken is null)
        {
            return false;
        }

        // The only way to verify the refresh token is still valid is to send a request with it
        // so we may as well save the results of the request since we're here anyway
        var modelFromReauthentication = await TryGenerateNewModelFromRefreshToken(modelFromStorage.RefreshToken);

        // Reauthentication failed so delete whatever is in storage
        if (modelFromReauthentication is null)
        {
            await ClearModel();
            return false;
        }
        
        await UpdateModel(modelFromReauthentication);
        return true;
    }

    public async Task<string> GetAccessToken()
        => (await GetModelFromStorageWithReauthenticationIfRequired()).AccessToken!;

    public async Task<string> GetIdToken()
        => (await GetModelFromStorageWithReauthenticationIfRequired()).IdToken!;

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
        var model = await GetModelFromStorage();

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

    async Task<StorageModel> GetModelFromStorageWithReauthenticationIfRequired()
    {
        var modelFromStorage = await GetModelFromStorage();

        if (!NeedToReauthenticateWithRefreshToken(modelFromStorage))
        {
            return modelFromStorage;
        }

        // If we don't have a refresh token, there's not much we can do other than logout and reload
        if (modelFromStorage.RefreshToken is null)
        {
            await ClearModel();
            throw new ArgumentNullException("Trying to generate new tokens but we don't have the refresh token");
        }

        var modelFromReauthentication = await TryGenerateNewModelFromRefreshToken(modelFromStorage.RefreshToken);

        // If we failed with the reauthentication, just logout and relaod
        if (modelFromReauthentication is null)
        {
            await ClearModel();
            throw new ArgumentNullException("Reauthentication failed with refresh token");
        }

        return modelFromReauthentication;
    }

    static bool NeedToReauthenticateWithRefreshToken(StorageModel model)
        => model.IdToken is null
        || model.AccessToken is null
        || model.AccessTokenExpires is null
        || model.AccessTokenExpires <= DateTime.Now;

    async Task<StorageModel?> TryGenerateNewModelFromRefreshToken(string refreshToken)
    {
        var loginApiResponse = await terrainAuthService.AttemptLoginWithRefreshToken(refreshToken);

        if (!loginApiResponse.Success)
        {
            Console.WriteLine(loginApiResponse.ErrorMessage);
            return null;
        }

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

    async Task<StorageModel> GetModelFromStorage()
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
