using Blazor.BrowserExtension.Pages;
using Microsoft.AspNetCore.Components;
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
}
