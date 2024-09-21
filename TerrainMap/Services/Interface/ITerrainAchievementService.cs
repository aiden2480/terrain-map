using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface ITerrainAchievementService
{
    Task<Achievement> GetAchievement(Approval approval);
}
