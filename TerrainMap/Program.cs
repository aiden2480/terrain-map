using Blazor.BrowserExtension;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Net.Http;
using TerrainMap;
using TerrainMap.Services;
using TerrainMap.Services.Interface;

// Create builder & configure browser extension
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.UseBrowserExtension(browserExtension =>
{
    if (browserExtension.Mode == BrowserExtensionMode.Background)
    {
        builder.RootComponents.AddBackgroundWorker<BackgroundWorker>();
    }
    else
    {
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
    }
});

// Add services & run
builder.Services
    .AddMudServices()
    .AddScoped<IStorageService, StorageService>()
    .AddScoped<ITerrainAchievementService, TerrainAchievementService>()
    .AddScoped<ITerrainApiClient, TerrainApiClient>()
    .AddScoped<ITerrainApprovalService, TerrainApprovalService>()
    .AddScoped<ITerrainAuthService, TerrainAuthService>()
    .AddScoped<ITerrainProfileService, TerrainProfileService>()
    .AddScoped<ITerrainTemplateService, TerrainTemplateService>()
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
