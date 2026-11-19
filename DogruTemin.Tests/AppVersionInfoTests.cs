using DogruTemin.Wpf;

namespace DogruTemin.Tests;

public class AppVersionInfoTests
{
    [Fact]
    public void VersionComesFromCentralBuildProperties()
    {
        Assert.StartsWith("4.4.1", AppVersionInfo.InformationalVersion, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("4.4.1", AppVersionInfo.ProductVersion);
        Assert.StartsWith("v4.4.1", AppVersionInfo.DisplayVersion, StringComparison.OrdinalIgnoreCase);
    }
}
