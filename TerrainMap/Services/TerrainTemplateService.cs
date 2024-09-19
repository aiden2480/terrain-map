using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainTemplateService(ITerrainApiClient terrainClient) : ITerrainTemplateService
{
    const string TemplatesUrl = "https://templates.terrain.scouts.com.au/{0}/{1}.json";

    public async Task<IEnumerable<ApprovalInput>> GetInputs(string templateId, int templateVersion)
    {
        var url = string.Format(TemplatesUrl, templateId, templateVersion);
        var response = await terrainClient.SendAuthenticatedRequest<TemplateApiResponse>(url);

        return response.Document.InputGroups.SelectMany(i => i.Inputs);
    }

    record TemplateApiResponse(
        [property: JsonPropertyName("document")] Document Document);

    record Document(
        [property: JsonPropertyName("input_groups")] IEnumerable<InputGroup> InputGroups);

    record InputGroup(
        [property: JsonPropertyName("inputs")] IEnumerable<ApprovalInput> Inputs);
}
