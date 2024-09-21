using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainApprovalService(ITerrainApiClient terrainClient) : ITerrainApprovalService
{
    const string PendingApprovalsUrl = "https://achievements.terrain.scouts.com.au/units/{0}/submissions?status=pending";
    const string FinalisedApprovalsUrl = "https://achievements.terrain.scouts.com.au/units/{0}/submissions?status=finalised";

    public async Task<IEnumerable<Approval>> GetPendingApprovals(string unitId)
        => await GetApprovals(PendingApprovalsUrl, unitId);

    public async Task<IEnumerable<Approval>> GetFinalisedApprovals(string unitId)
        => await GetApprovals(FinalisedApprovalsUrl, unitId);

    async Task<IEnumerable<Approval>> GetApprovals(string formatUrl, string unitId)
    {
        var url = string.Format(formatUrl, unitId);
        var response = await terrainClient.SendGet<GetApprovalsResponse>(url);

        return response.Results;
    }

    record GetApprovalsResponse(
        [property: JsonPropertyName("results")] IEnumerable<Approval> Results);

    public string GetApprovalDescription(Approval approval)
        => approval.Achievement.Type switch
        {
            "intro_scouting" => "Introduction to Scouting",
            "intro_section" => "Introduction to Section",
            "course_reflection" => "Personal Development Course",
            "adventurous_journey" => $"Adventurous Journey ({approval.Submission.Type})",
            "personal_reflection" => "Personal Reflection",
            "peak_award" => "Peak Award",

            "special_interest_area" => GetSIADescription(approval),
            "outdoor_adventure_skill" => GetOASDescription(approval),
            "milestone" => $"Milestone {approval.Achievement.Meta.Stage}",

            _ => "Unknown achievement",
        };

    static string GetSIADescription(Approval approval)
        => approval.Achievement.Meta.SIAArea switch
        {
            "sia_adventure_sport" => "Adventure & Sport",
            "sia_art_literature" => "Arts & Literature",
            "sia_better_world" => "Creating a Better World",
            "sia_environment" => "Environment",
            "sia_growth_development" => "Growth & Development",
            "sia_stem_innovation" => "STEM & Innovation",

            _ => "Unknown",
        } + " SIA";

    static string GetOASDescription(Approval approval)
    {
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        var branch = approval.Achievement.Meta.Branch?.Replace("-", " ");
        var stage = approval.Achievement.Meta.Stage;

        return !string.IsNullOrEmpty(branch)
            ? textInfo.ToTitleCase($"{branch} stage {stage}")
            : "Outdoor Adventure Skill";
    }
}
