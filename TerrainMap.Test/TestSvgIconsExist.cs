using TerrainMap.Models;

namespace TerrainMap.Test;

public class TestSvgIconsExist
{
    [Test]
    public void TestSvgIconExists([Values] SvgIcon icon)
    {
        var iconName = icon.ToString().ToLower();
        var relativePath = Path.Combine("wwwroot", "icons", iconName + ".svg");

        Assert.That(relativePath, Does.Exist);
    }
}
