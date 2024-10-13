using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface ITerrainApprovalService
{
    /// <summary>
    /// Get all pending approvals for a particular unit
    /// </summary>
    Task<IEnumerable<Approval>> GetPendingApprovals(string unitId);

    /// <summary>
    /// Get all finalised approvals for a particular unit
    /// </summary>
    Task<IEnumerable<Approval>> GetFinalisedApprovals(string unitId);

    /// <summary>
    /// Gets the friendly name for an approval
    /// </summary>
    (string Description, SvgIcon Icon) GetApprovalDescriptionAndSvg(Approval approval);
}
