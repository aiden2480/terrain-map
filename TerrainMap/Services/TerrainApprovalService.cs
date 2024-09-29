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
    const string GetApprovalSubmissionUrl = "https://achievements.terrain.scouts.com.au/units/{0}/submissions/{1}";

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

    public async Task<Approval> RefreshApproval(string unitId, Approval approval)
    {
        var url = string.Format(GetApprovalSubmissionUrl, unitId, approval.Submission.Id);
        var response = await terrainClient.SendGet<Approval>(url);

        return response;
    }

    record GetApprovalsResponse(
        [property: JsonPropertyName("results")] IEnumerable<Approval> Results);

    public (string, SvgIcon) GetApprovalDescriptionAndSvg(Approval approval)
        => approval.Achievement.Type switch
        {
            "intro_scouting" => ("Introduction to Scouting", SvgIcon.FleurDeLis),
            "intro_section" => ("Introduction to Section", SvgIcon.ShintoShrine),
            "course_reflection" => ("Personal Development Course", SvgIcon.Books),
            "adventurous_journey" => ($"Adventurous Journey ({approval.Submission.Type})", SvgIcon.Rocket),
            "personal_reflection" => ("Personal Reflection", SvgIcon.YinYang),
            "peak_award" => ("Peak Award", SvgIcon.GlowingStar),

            "special_interest_area" => GetSIA(approval),
            "outdoor_adventure_skill" => GetOAS(approval),
            "milestone" => ($"Milestone {approval.Achievement.Meta.Stage}", SvgIcon.Gemstone),

            _ => ("Unknown achievement", SvgIcon.Trophy),
        };

    static (string, SvgIcon) GetSIA(Approval approval)
    {
        (string description, SvgIcon icon) = approval.Achievement.Meta.SIAArea switch
        {
            "sia_adventure_sport" => ("Adventure & Sport", SvgIcon.Basketball),
            "sia_art_literature" => ("Arts & Literature", SvgIcon.PerformingArts),
            "sia_better_world" => ("Creating a Better World", SvgIcon.Globe),
            "sia_environment" => ("Environment", SvgIcon.Recycle),
            "sia_growth_development" => ("Growth & Development", SvgIcon.Seedling),
            "sia_stem_innovation" => ("STEM & Innovation", SvgIcon.MagnifyingGlass),

            _ => ("Unknown", SvgIcon.Trophy),
        };

        return ($"{description} SIA ({approval.Submission.Type})", icon);
    }

    static (string, SvgIcon) GetOAS(Approval approval)
    {
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        var branch = approval.Achievement.Meta.Branch?.Replace("-", " ");
        var stage = approval.Achievement.Meta.Stage;

        if (approval.Achievement.Meta is null || string.IsNullOrEmpty(branch))
        {
            return ("Outdoor Adventure Skill", SvgIcon.Trophy);
        }

        var description = textInfo.ToTitleCase($"{branch} stage {stage}");
        var icon = approval.Achievement.Meta.Stream switch
        {
            "alpine" => SvgIcon.Snowman,
            "aquatics" => SvgIcon.TropicalFish,
            "boating" => SvgIcon.Sailboat,
            "bushcraft" => SvgIcon.Compass,
            "bushwalking" => SvgIcon.HikingBoot,
            "camping" => SvgIcon.Camping,
            "cycling" => SvgIcon.Bicycle,
            "paddling" => SvgIcon.Paddling,
            "vertical" => SvgIcon.Climbing,

            _ => SvgIcon.Trophy
        };

        return (description, icon);
    }
}
