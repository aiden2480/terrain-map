using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Pages;

public partial class Popup : BasePage
{
    [Inject]
    public required IStorageService StorageService { get; set; }

    [Inject]
    public required ITerrainApprovalService TerrainApprovalService { get; set; }

    [Inject]
    public required ITerrainProfileService TerrainProfileService { get; set; }

    async Task<Profile?> GetFirstProfileInUnitCouncil(bool forceRefreshCache = false)
        => (await GetProfiles(forceRefreshCache))
            .Where(p => p.Unit is not null)
            .Where(p => p.Unit!.Roles.Contains("unit-council"))
            .FirstOrDefault();

    async Task<IEnumerable<Profile>> GetProfiles(bool forceRefreshCache)
    {
        var profiles = await StorageService.GetProfilesFromStorage();

        if (profiles is null || forceRefreshCache)
        {
            profiles = await TerrainProfileService.GetProfiles();
            await StorageService.SetProfiles(profiles);
        }

        return profiles;
    }

    async Task<IEnumerable<Approval>> GetPendingApprovals(Unit unit)
    {
        var pending = await TerrainApprovalService.GetPendingApprovals(unit.Id);

        return pending
            .Where(a => a.Submission.Type == SubmissionType.Approval || a.Submission.Type == SubmissionType.Review)
            .OrderBy(a => a.Submission.Date);
    }

    async Task<IEnumerable<Approval>> GetPendingAwards(Unit unit)
    {
        var pending = await TerrainApprovalService.GetPendingApprovals(unit.Id);

        return pending
            .Where(a => a.Submission.Type == SubmissionType.Award)
            .OrderBy(a => a.Submission.Date);
    }

    async Task<IEnumerable<Approval>> GetFinalisedApprovals(Unit unit)
    {
        var finalised = await TerrainApprovalService.GetFinalisedApprovals(unit.Id);

        return finalised
            .Where(a => a.Submission.Type == SubmissionType.Approval || a.Submission.Type == SubmissionType.Review)
            .Where(a => a.Submission.Date >= DateTime.UtcNow.AddDays(-90))
            .OrderByDescending(a => a.Submission.Date);
    }
}
