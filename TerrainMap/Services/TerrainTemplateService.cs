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

    async Task<IEnumerable<ApprovalInput>> GetInputs(string templateId, int templateVersion)
    {
        var url = string.Format(TemplatesUrl, templateId, templateVersion);
        var response = await terrainClient.SendGet<TemplateApiResponse>(url);

        return response.Document
            .SelectMany(d => d.InputGroups)
            .SelectMany(i => i.Inputs)
            .Where(i => i.Type != "file_uploader")
            .Where(i => i.Id != "logbook_up_to_date");
    }

    public async Task<IEnumerable<ApprovalInput>> GetInputs(Approval approval, Achievement achievement)
    {
        // For some reason, SIA reviews do not have an updated template field, so we must overwrite it manually
        var template = (approval.Submission.Type == "review" && approval.Achievement.Type == "special_interest_area")
            ? achievement.Template.Replace("plan", "review")
            : achievement.Template;

        return await GetInputs(template, achievement.Version);
    }

    record TemplateApiResponse(
        [property: JsonPropertyName("document")] IEnumerable<DocumentItem> Document);

    record DocumentItem(
        [property: JsonPropertyName("input_groups")] IEnumerable<InputGroup> InputGroups);

    record InputGroup(
        [property: JsonPropertyName("inputs")] IEnumerable<ApprovalInput> Inputs);
}
