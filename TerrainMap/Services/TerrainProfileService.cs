using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainProfileService(ITerrainApiClient terrainClient) : ITerrainProfileService
{
    const string ProfilesUrl = "https://members.terrain.scouts.com.au/profiles";

    public async Task<IEnumerable<Profile>> GetProfiles()
    {
        var response = await terrainClient.SendAuthenticatedRequest<GetProfilesResponse>(ProfilesUrl);

        return response.Profiles;
    }

    record GetProfilesResponse(
        [property: JsonPropertyName("profiles")] IEnumerable<Profile> Profiles,
        [property: JsonPropertyName("username")] string Username);
}
