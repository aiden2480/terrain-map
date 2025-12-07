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
    public required Func<Unit, Task<IEnumerable<Approval>>> ApprovalsAccessor { get; init; }

    [Parameter]
    public bool IsReadOnly { get; init; }

    private IEnumerable<Approval>? approvals;

    protected override async Task OnInitializedAsync()
    {
        var allApprovals = await ApprovalsAccessor.Invoke(CurrentProfile.Unit!);

        // 30 results is plenty, we're not trying to crash the browser here
        approvals = allApprovals.Take(30);
    }
}
