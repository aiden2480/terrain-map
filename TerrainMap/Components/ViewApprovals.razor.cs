using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Components;

public partial class ViewApprovals : ComponentBase
{
    [Inject]
    public required ITerrainApprovalService TerrainApprovalService { get; set; }

    [Parameter]
    [EditorRequired]
    public required Profile CurrentProfile { get; init; }

    [Parameter]
    [EditorRequired]
    public required Func<Approval, bool> ApprovalsPredicate { get; init; }

    private IEnumerable<Approval>? approvals;

    protected override async Task OnInitializedAsync()
    {
        approvals = (await TerrainApprovalService.GetPendingApprovals(CurrentProfile.Unit!.Id))
            .Where(ApprovalsPredicate)
            .OrderBy(a => a.Submission.Date);
    }
}
