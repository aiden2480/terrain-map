using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.IO;
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
        Panels = PendingApprovals
            .Where(a => a.Submission.Type != "award") // TODO can handle awards separately later
            .Select(a => new ApprovalPanel
            {
                Approval = a,
            }).ToList();
    }

    async Task ExpandedChanged(ApprovalPanel panel, bool isOpen)
    {
        if (isOpen && !panel.DataIsLoaded)
        {
            await panel.LoadData(TerrainAchievementService, TerrainTemplateService);
        }
    }

    string GetPanelText(Approval a)
        => $"{a.Member.FirstName}'s {TerrainApprovalService.GetApprovalDescriptionAndSvg(a).Description}";

    string GetPanelIconStyle(Approval a)
    {
        var icon = TerrainApprovalService.GetApprovalDescriptionAndSvg(a).Icon;
        var iconUrl = Path.Combine("icons", icon.ToString().ToLower() + ".svg");

        return $"content: url({iconUrl}); font-size: 1.9rem";
    }

    static int GetApproveCount(Approval approval)
        => approval.Submission.ActionedBy.Count(a => a.Outcome == "approved");

    static string PluralApproveText(Approval approval)
        => GetApproveCount(approval) == 1 ? "approve" : "approves";

    static int GetImproveCount(Approval approval)
        => approval.Submission.ActionedBy.Count(a => a.Outcome == "rejected");

    static string PluralImproveText(Approval approval)
        => GetImproveCount(approval) == 1 ? "improve" : "improves";

    bool CanActionApproval(Approval approval)
        => !SubmittedByCurrentUser(approval) && !AlreadyActionedByCurrentUser(approval);

    bool SubmittedByCurrentUser(Approval approval)
        => approval.Member.Id == CurrentProfile.Member.Id;

    bool AlreadyActionedByCurrentUser(Approval approval)
        => approval.Submission.ActionedBy
            .Select(a => a.Id)
            .Contains(CurrentProfile.Member.Id);
     
    async Task ApproveAchievement(ApprovalPanel panel)
    {
        var continueAction = await ShowCommentDialog(panel.Approval, true);

        if (continueAction)
        {
            await TerrainAchievementService.ApproveSubmission(panel.Approval.Submission, comment);
            panel.Approval = await TerrainApprovalService.RefreshApproval(CurrentProfile.Unit!.Id, panel.Approval);
        }
    }

    async Task ImproveAchievement(ApprovalPanel panel)
    {
        var continueAction = await ShowCommentDialog(panel.Approval, false);

        if (continueAction)
        {
            await TerrainAchievementService.ImproveSubmission(panel.Approval.Submission, comment);
            panel.Approval = await TerrainApprovalService.RefreshApproval(CurrentProfile.Unit!.Id, panel.Approval);
        }
    } 

    async Task<bool> ShowCommentDialog(Approval approval, bool isApproval)
    {
        var parameters = new DialogParameters<ApprovalCommentDialog>
        {
            { d => d.IsApproval, isApproval },
            { d => d.Comment, comment }
        };

        var title = isApproval
            ? $"Approve {GetPanelText(approval)}?"
            : $"Improve {GetPanelText(approval)}?";

        var dialog = await DialogService.ShowAsync<ApprovalCommentDialog>(title, parameters);
        var result = await dialog.Result;

        return !result!.Canceled;
    }

    class ApprovalPanel
    {
        public required Approval Approval { get; set; }

        public Achievement Achievement = null!;

        public IEnumerable<ApprovalInput> Inputs = [];

        public bool DataIsLoaded => Achievement is not null;

        public async Task LoadData(ITerrainAchievementService achievementService, ITerrainTemplateService templateService)
        {
            Achievement = await achievementService.GetAchievement(Approval);
            Inputs = await templateService.GetInputs(Achievement);
        }
    }
}
