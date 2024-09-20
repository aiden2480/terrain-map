using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface ITerrainTemplateService
{
    Task<Dictionary<string, ApprovalInput>> GetInputs(string templateId, int templateVersion);

    Task<Dictionary<string, ApprovalInput>> GetInputs(Achievement achievement);
}
