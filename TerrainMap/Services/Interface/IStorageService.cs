using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface IStorageService
{
    /// <summary>
    /// Checks that the user is authenticated by validating and reauthenticating the refresh token.
    /// If reauthentication fails, clears storage data since we know it's invalid.
    /// </summary>
    Task<bool> IsAuthenticated();

    /// <returns>The current user's access token. Ensure the user <see cref="IsAuthenticated"/> before invoking</returns>
    Task<string> GetAccessToken();

    /// <returns>The current user's id token. Ensure the user <see cref="IsAuthenticated"/> before invoking</returns>
    Task<string> GetIdToken();

    /// <summary>
    /// Updates the cache with a response from the Terrain API after
    /// auethenticating with a username and password
    /// </summary>
    /// <param name="loginApiResponse">The API response. Must contain refresh token</param>
    Task UpdateFromLoginApiResponse(LoginApiResponse loginApiResponse);

    Task<IEnumerable<Profile>?> GetProfilesFromStorage();

    Task SetProfiles(IEnumerable<Profile> profiles);
}
