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
    Task<TResult> SendGet<TResult>(string url);

    /// <summary>
    /// Send a POST request to the nominated endpoint which has been authenticated with the Id token
    /// </summary>
    /// <typeparam name="TContent">The content type</typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="url">The fully qualified endpoint url</param>
    /// <param name="content">The content which will be serialised into the content field of the request</param>
    /// <returns>The deserialised response</returns>
    Task<TResult> SendPost<TContent, TResult>(string url, TContent content);

    /// <summary>
    /// Sends a POST request without reading the response
    /// </summary>
    Task SendPostVoid<TContent>(string url, TContent content);
}
