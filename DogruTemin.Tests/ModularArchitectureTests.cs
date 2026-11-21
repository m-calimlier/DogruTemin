using DogruTemin.Wpf.Features;
using Xunit;
namespace DogruTemin.Tests;
public sealed class ModularArchitectureTests
{
    [Fact]
    public void EveryFeatureHasItsOwnControllerAndDoesNotInheritTheWindow()
    {
        var types = typeof(WorkspaceContext).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(FeatureModule).IsAssignableFrom(t)).ToArray();
        Assert.Equal(9, types.Length);
        Assert.All(types, t => Assert.False(typeof(System.Windows.Window).IsAssignableFrom(t)));
        Assert.All(types, t => Assert.DoesNotContain(t.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), f => f.FieldType == typeof(DogruTemin.Wpf.MainWindow)));
    }
}
