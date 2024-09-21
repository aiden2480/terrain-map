using System.Net.Http;
using System.Threading.Tasks;

namespace TerrainMap.Services.Interface;

public interface ITerrainApiClient
{
    /// <summary>
    /// Send a GET request to the nominated endpoint which has been authenticated with the Id token
    /// </summary>
    /// <typeparam name="TResult">The object type to deserialise the response to</typeparam>
    /// <param name="url">The fully qualified endpoint url</param>
    /// <returns>The deserialised response</returns>
    Task<TResult> SendAuthenticatedRequest<TResult>(string url);

    /// <summary>
    /// Send a <paramref name="method"/> request to the nominated endpoint which has been authenticated with the Id token.
    /// Otherwise same as <see cref="SendAuthenticatedRequest{TResult}(string)"/>
    /// </summary>
    Task<TResult> SendAuthenticatedRequest<TResult>(string url, HttpMethod method);
}
