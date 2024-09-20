using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Components;

public partial class ViewPendingApprovals : ComponentBase
{
    [Inject] public required ITerrainApprovalService TerrainApprovalService { get; set; }

    [Inject] public required ITerrainAchievementService TerrainAchievementService { get; set; }

    [Inject] public required ITerrainTemplateService TerrainTemplateService { get; set; }

    [Inject] public required IDialogService DialogService { get; set; }

    [Parameter, EditorRequired] public required Profile CurrentProfile { get; set; }

    [Parameter, EditorRequired] public required IEnumerable<Approval> PendingApprovals { get; set; }

    private string comment = string.Empty;

    private IList<ApprovalPanel> Panels = default!;

    protected override void OnInitialized()
    {
        Panels = PendingApprovals.Select(a => new ApprovalPanel
        {
            Approval = a,
        }).ToList();
    }

    async Task ExpandedChanged(ApprovalPanel panel, bool isOpen)
    {
        if (isOpen && !panel.DataIsLoaded)
        {
            panel.Achievement = await TerrainAchievementService.GetAchievement(panel.Approval);
            panel.Inputs = await TerrainTemplateService.GetInputs(panel.Achievement);
        }
    }

    string GetPanelText(Approval a)
        => $"{a.Member.FirstName}'s {TerrainApprovalService.GetApprovalDescription(a)}";

    static int GetApproveCount(Approval approval)
        => approval.Submission.ActionedBy.Count(a => a.Outcome == "approved");

    static string PluralApproveText(Approval approval)
        => GetApproveCount(approval) == 1 ? "approve" : "approves";

    static int GetImproveCount(Approval approval)
        => approval.Submission.ActionedBy.Count(a => a.Outcome == "rejected");

    static string PluralImproveText(Approval approval)
        => GetImproveCount(approval) == 1 ? "improve" : "improves";

    bool AlreadyActionedByCurrentUser(Approval approval)
        => approval.Submission.ActionedBy
            .Select(a => a.Id)
            .Contains(CurrentProfile.Member.Id);

    async Task ApproveDialog(Approval approval)
        => Console.WriteLine(await ShowApprovalCommentDialog($"Approve {GetPanelText(approval)}?", true));

    async Task ImproveDialog(Approval approval)
        => Console.WriteLine(await ShowApprovalCommentDialog($"Improve {GetPanelText(approval)}?", false)); 

    async Task<bool> ShowApprovalCommentDialog(string title, bool isApproval)
    {
        var parameters = new DialogParameters<ApprovalCommentDialog>
        {
            { d => d.IsApproval, isApproval },
            { d => d.Comment, comment }
        };

        var dialog = await DialogService.ShowAsync<ApprovalCommentDialog>(title, parameters);
        var result = await dialog.Result;

        return !result!.Canceled;
    }

    class ApprovalPanel
    {
        public required Approval Approval { get; init; }

        public Achievement Achievement = null!;

        public Dictionary<string, ApprovalInput> Inputs = [];

        public bool DataIsLoaded => Achievement is not null;
    }
}
