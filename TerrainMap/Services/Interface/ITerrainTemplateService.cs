using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface ITerrainTemplateService
{
    Task<IEnumerable<ApprovalInput>> GetInputs(string templateId, int templateVersion);
}
