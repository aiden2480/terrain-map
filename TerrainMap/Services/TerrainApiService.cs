using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainApiService(HttpClient httpClient, ILocalAuthService authService) : ITerrainApiService
{
    const string ProfilesUrl = "https://members.terrain.scouts.com.au/profiles";
    const string PendingApprovalsUrl = "https://achievements.terrain.scouts.com.au/units/{0}/submissions?status=pending";
    const string FinalisedApprovalsUrl = "https://achievements.terrain.scouts.com.au/units/{0}/submissions?status=finalised";

    #region GetProfiles

    public async Task<IEnumerable<Profile>> GetProfiles()
    {
        var response = await SendAuthenticatedRequest<GetProfilesResponse>(ProfilesUrl);

        return response.Profiles;
    }

    record GetProfilesResponse(
        [property: JsonPropertyName("profiles")] IEnumerable<Profile> Profiles,
        [property: JsonPropertyName("username")] string Username);

    #endregion

    #region GetApprovals

    public async Task<IEnumerable<Approval>> GetPendingApprovals(string unitId)
        => await GetApprovals(PendingApprovalsUrl, unitId);

    public async Task<IEnumerable<Approval>> GetFinalisedApprovals(string unitId)
        => await GetApprovals(FinalisedApprovalsUrl, unitId);

    async Task<IEnumerable<Approval>> GetApprovals(string formatUrl, string unitId)
    {
        var url = string.Format(formatUrl, unitId);
        var response = await SendAuthenticatedRequest<GetApprovalsResponse>(url);

        return response.Results;
    }

    record GetApprovalsResponse([property: JsonPropertyName("results")] IEnumerable<Approval> Results);

    #endregion

    async Task<TResult> SendAuthenticatedRequest<TResult>(string url)
    {
        var request = await GetAuthenticatedRequest(url, HttpMethod.Get);
        var response = await httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();
        var responseParsed = JsonSerializer.Deserialize<TResult>(responseText);

        Debug.Assert(responseParsed is not null);
        return responseParsed;
    }

    async Task<HttpRequestMessage> GetAuthenticatedRequest(string url, HttpMethod method)
    {
        var request = new HttpRequestMessage(method, url);
        var idToken = await authService.GetIdToken();

        request.Headers.Authorization = new AuthenticationHeaderValue(idToken);
        return request;
    }
}
