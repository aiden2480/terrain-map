using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface ITerrainApiService
{
    /// <summary>
    /// Get all profiles available to the current user
    /// </summary>
    Task<IEnumerable<Profile>> GetProfiles();

    /// <summary>
    /// Get all pending approvals for a particular unit
    /// </summary>
    Task<IEnumerable<Approval>> GetPendingApprovals(string unitId);

    /// <summary>
    /// Get all finalised approvals for a particular unit
    /// </summary>
    Task<IEnumerable<Approval>> GetFinalisedApprovals(string unitId);
}
