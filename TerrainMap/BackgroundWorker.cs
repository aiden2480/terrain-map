using Blazor.BrowserExtension;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;
using WebExtensions.Net.ActionNs;

namespace TerrainMap;

public partial class BackgroundWorker : BackgroundWorkerBase
{
    [Inject] public required IStorageService StorageService { get; set; }

    [Inject] public required ITerrainApprovalService TerrainApprovalService { get; set; }

    [BackgroundWorkerMain]
    public override void Main()
    {
        WebExtensions.Runtime.OnInstalled.AddListener(StartupListener);
        WebExtensions.Runtime.OnStartup.AddListener(StartupListener);
    }

    async Task StartupListener()
    {
        // We can make the popup faster by ensuring the refresh token is valid on startup
        // as opposed to on popup.
        if (!await StorageService.EnsureAuthenticated(andValidateRefreshToken: true))
        {
            return;
        }

        var profiles = await StorageService.GetUnitCouncilProfilesFromStorage();
        var profile = profiles.FirstOrDefault();

        if (profile is null)
        {
            return;
        }

        var pendingApprovals = (await TerrainApprovalService.GetPendingApprovals(profile.Unit!.Id))
            .Where(a => a.Submission.Type == SubmissionType.Approval || a.Submission.Type == SubmissionType.Review);

        if (!pendingApprovals.Any())
        {
            return;
        }

        await WebExtensions.Action.SetBadgeBackgroundColor(new SetBadgeBackgroundColorDetails { Color = GetBadgeColor() });
        await WebExtensions.Action.SetBadgeText(new SetBadgeTextDetails { Text = pendingApprovals.Count().ToString() });
    }

    static string GetBadgeColor()
    {
        #if DEBUG
            return "#A41F37";
        #else
            return "#212D65";
        #endif
    }
}
