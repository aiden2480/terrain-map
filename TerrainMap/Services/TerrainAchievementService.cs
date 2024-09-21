using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainAchievementService(ITerrainApiClient terrainApi) : ITerrainAchievementService
{
    const string GetAchievementUrl = "https://achievements.terrain.scouts.com.au/members/{0}/achievements/{1}";

    async Task<Achievement> GetAchievement(string memberId, string achievementId)
    {
        var url = string.Format(GetAchievementUrl, memberId, achievementId);
        var response = await terrainApi.SendAuthenticatedRequest<Achievement>(url);

        return response;
    }

    public async Task<Achievement> GetAchievement(Approval approval)
        => await GetAchievement(approval.Member.Id, approval.Achievement.Id);
}
