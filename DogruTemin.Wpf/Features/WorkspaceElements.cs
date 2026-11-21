using System.Windows;
using System.Windows.Controls;

namespace DogruTemin.Wpf.Features;
internal static class WorkspaceElements
{
    internal static object? Find(Window window, string name)
    {
        // Page controls create independent namescopes. Search them explicitly, including collapsed pages.
        if (window.Content is not DependencyObject root)
            return null;
        return FindIn(root, name);
    }

    private static object? FindIn(DependencyObject node, string name)
    {
        if (node is FrameworkElement element)
        {
            if (element.Name == name)
                return element;
            if (element is UserControl && element.FindName(name)is object match)
                return match;
        }

        foreach (var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>())
            if (FindIn(child, name)is object match)
                return match;
        return null;
    }
}

internal interface IWorkspaceActionSink
{
    void HandleWorkspaceAction(string action, object? sender, EventArgs args);
}

internal static class WorkspaceActionRouter
{
    internal static void Dispatch(FrameworkElement source, string action, object? sender, EventArgs args)
    {
        // InitializeComponent may fire change events before the page belongs to its window.
        if (Window.GetWindow(source)is IWorkspaceActionSink sink)
            sink.HandleWorkspaceAction(action, sender, args);
    }
}
