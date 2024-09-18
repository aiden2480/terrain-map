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

    private string comment = string.Empty;
    
    private string SubmitText
        => IsApproval ? "Approve" : "Improve";

    private Color SubmitColor
        => IsApproval ? Color.Primary : Color.Secondary;

    private void Submit()
        => Dialog.Close(comment);

    private void Cancel()
        => Dialog.Cancel();
}
