using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Components;

public partial class PendingApprovalPanel : ComponentBase
{
    [Inject]
    public required ITerrainApprovalService TerrainApprovalService { get; set; }

    [Inject]
    public required ITerrainAchievementService TerrainAchievementService { get; set; }

    [Inject]
    public required ITerrainTemplateService TerrainTemplateService { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Parameter]
    [EditorRequired]
    public required Approval Approval { get; set; }

    [Parameter]
    [EditorRequired]
    public required Profile CurrentProfile { get; init; }

    Achievement Achievement = null!;

    IEnumerable<ApprovalInput> Inputs = [];

    string comment = string.Empty;

    bool actionedByCurrentUser = false;

    bool DataIsLoaded
        => Achievement is not null && Inputs is not null;

    string PanelText
        => $"{Approval.Member.FirstName}'s {TerrainApprovalService.GetApprovalDescriptionAndSvg(Approval).Description}";

    int ApproveCount
        => Approval.Submission.ActionedBy.Count(a => a.Outcome == "approved");

    string PluralApproveText
        => ApproveCount == 1 ? "approve" : "approves";

    int ImproveCount
        => Approval.Submission.ActionedBy.Count(a => a.Outcome == "rejected");

    string PluralImproveText
        => ImproveCount == 1 ? "improve" : "improves";

    bool CanActionApproval
        => !SubmittedByCurrentUser && !AlreadyActionedByCurrentUser;

    bool SubmittedByCurrentUser
        => Approval.Member.Id == CurrentProfile.Member.Id;

    bool AlreadyActionedByCurrentUser
        => actionedByCurrentUser || Approval.Submission.ActionedBy
             .Select(a => a.Id)
             .Contains(CurrentProfile.Member.Id);

    async Task ExpandedChanged(bool isOpen)
    {
        if (isOpen && !DataIsLoaded)
        {
            Achievement = await TerrainAchievementService.GetAchievement(Approval);
            Inputs = await TerrainTemplateService.GetInputs(Achievement);
        }
    }

    async Task ApproveAchievement()
    {
        var continueAction = await ShowCommentDialog(true);

        if (continueAction)
        {
             await TerrainAchievementService.ApproveSubmission(Approval.Submission, comment);
            actionedByCurrentUser = true;
        }
    }

    async Task ImproveAchievement()
    {
        var continueAction = await ShowCommentDialog(false);

        if (continueAction)
        {
             await TerrainAchievementService.ImproveSubmission(Approval.Submission, comment);
            actionedByCurrentUser = true;
        }
    }

    async Task<bool> ShowCommentDialog(bool isApproval)
    {
        var parameters = new DialogParameters<ApprovalCommentDialog>
        {
            { d => d.IsApproval, isApproval },
            { d => d.Comment, comment }
        };

        var title = isApproval ? $"Approve {PanelText}?" : $"Improve {PanelText}?";
        var dialog = await DialogService.ShowAsync<ApprovalCommentDialog>(title, parameters);
        var result = await dialog.Result;

        return !result!.Canceled;
    }
}
