using Blazor.BrowserExtension;
using System.Threading.Tasks;
using WebExtensions.Net.ActionNs;

namespace TerrainMap;

public partial class BackgroundWorker : BackgroundWorkerBase
{
    [BackgroundWorkerMain]
    public override void Main()
    {
#if DEBUG
        WebExtensions.Runtime.OnInstalled.AddListener(SetBetaBadge);
        WebExtensions.Runtime.OnStartup.AddListener(SetBetaBadge);
#endif
    }

    async Task SetBetaBadge()
    {
        await WebExtensions.Action.SetBadgeText(new SetBadgeTextDetails { Text = "B" });
        await WebExtensions.Action.SetBadgeBackgroundColor(new SetBadgeBackgroundColorDetails { Color = "#212d65" });
    }
}
