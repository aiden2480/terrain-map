using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Components;

public partial class PendingAwardPanel : ComponentBase
{
    [Inject]
    public required ITerrainApprovalService TerrainApprovalService { get; set; }

    [Inject]
    public required ITerrainAchievementService TerrainAchievementService { get; set; }

    [Parameter]
    [EditorRequired]
    public required Approval Approval { get; init; }

    DateTime? awardedDate = DateTime.Today;

    bool submitted = false;

    string PanelText
        => $"{Approval.Member.FirstName}'s {TerrainApprovalService.GetApprovalDescriptionAndSvg(Approval).Description} Award";

    async Task Submit()
    {
        if (awardedDate is not null)
        {
            await TerrainAchievementService.AwardSubmission(Approval.Submission, Convert.ToDateTime(awardedDate));
            submitted = true;
        }
    }
}
