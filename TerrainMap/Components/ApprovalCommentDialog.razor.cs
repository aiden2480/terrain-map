using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TerrainMap.Components;

public partial class ApprovalCommentDialog : ComponentBase
{
    [CascadingParameter]
    public required MudDialogInstance Dialog { get; init; }

    [Parameter]
    [EditorRequired]
    public required bool IsApproval { get; init; }

    [Parameter]
    [EditorRequired]
    public required string? Comment { get; init; }

    private string DialogContent
        => !string.IsNullOrWhiteSpace(Comment)
            ? $"{ActionVerb} with comment \"{Comment}\"?"
            : $"{ActionVerb} without comment?";
    
    private string ActionVerb
        => IsApproval ? "Approve" : "Improve";

    private Color SubmitColor
        => IsApproval ? Color.Primary : Color.Secondary;

    private void Submit()
        => Dialog.Close();

    private void Cancel()
        => Dialog.Cancel();
}
