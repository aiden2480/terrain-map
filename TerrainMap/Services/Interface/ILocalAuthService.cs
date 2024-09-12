using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface ILocalAuthService
{
    /// <summary>
    /// Has a refresh token been set which can be used to find any other tokens
    /// </summary>
    Task<bool> IsAuthenticated();

    /// <returns>The current user's access token</returns>
    Task<string> GetAccessToken();

    /// <returns>The current user's id token</returns>
    Task<string> GetIdToken();

    /// <summary>
    /// Updates the cache with a response from the Terrain API after
    /// auethenticating with a username and password
    /// </summary>
    /// <param name="loginApiResponse">The API response. Must contain refresh token</param>
    Task UpdateFromLoginApiResponse(LoginApiResponse loginApiResponse);
}
