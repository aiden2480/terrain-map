using Microsoft.AspNetCore.Components;
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

    [Parameter]
    [EditorRequired]
    public required Profile CurrentProfile { get; init; }

    private IEnumerable<Approval>? pendingApprovals;

    protected override async Task OnInitializedAsync()
    {
        // Show pending awards at the top, then order by date ascending (oldest submissions first)
        pendingApprovals = (await TerrainApprovalService.GetPendingApprovals(CurrentProfile.Unit!.Id))
            .OrderByDescending(a => a.Submission.Type == "award")
            .ThenBy(a => a.Submission.Date);
    }
}
