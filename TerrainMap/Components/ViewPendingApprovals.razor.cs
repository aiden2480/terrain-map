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
    [Inject]
    public required ITerrainApprovalService TerrainApprovalService { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Parameter]
    [EditorRequired]
    public required Profile CurrentProfile { get; set; }

    [Parameter]
    [EditorRequired]
    public required IEnumerable<Approval> PendingApprovals { get; set; }

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
        => await ShowApprovalCommentDialog($"Approve {GetPanelText(approval)}?", true);

    async Task<string?> ImproveDialog(Approval approval)
        => await ShowApprovalCommentDialog($"Reject {GetPanelText(approval)}?", false); 

    async Task<string?> ShowApprovalCommentDialog(string title, bool isApproval)
    {
        var parameters = new DialogParameters<ApprovalCommentDialog>
        {
            { d => d.IsApproval, isApproval }
        };

        var dialog = await DialogService.ShowAsync<ApprovalCommentDialog>(title, parameters);
        var result = await dialog.Result;
        var comment = result?.Data as string;

        return string.IsNullOrWhiteSpace(comment) ? null : comment;
    }
}
