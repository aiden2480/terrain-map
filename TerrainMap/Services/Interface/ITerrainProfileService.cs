using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface ITerrainProfileService
{
    /// <summary>
    /// Get all profiles available to the current user
    /// </summary>
    Task<IEnumerable<Profile>> GetProfiles();
}
