using System.Reflection;
using DogruTemin.Wpf;
namespace DogruTemin.Tests;
internal static class WorkspaceTestDriver
{
 internal static object Invoke(MainWindow window, string method, object[]? args)
 {
  foreach (var module in window.Features.Modules)
   if (module.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) is MethodInfo action)
    return action.Invoke(module, args)!;
  throw new MissingMethodException(method);
 }
}
