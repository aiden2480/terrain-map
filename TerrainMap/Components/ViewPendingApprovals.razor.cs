using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Components;

public partial class ViewPendingApprovals : ComponentBase
{
    [Inject]
    public required ITerrainApprovalService TerrainApprovalService { get; set; }

    [Parameter]
    [EditorRequired]
    public required Profile CurrentProfile { get; init; }

    private IEnumerable<Approval>? pendingApprovals;

    protected override async Task OnInitializedAsync()
    {
        pendingApprovals = await TerrainApprovalService.GetPendingApprovals(CurrentProfile.Unit!.Id);
    }
}
