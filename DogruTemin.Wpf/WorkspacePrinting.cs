using System;
using System.Windows;
using System.Windows.Controls;

namespace DogruTemin.Wpf;

internal static class WorkspacePrinting
{
    public static bool ConfirmPhysicalQueue(PrintDialog dialog)
    {
        if (dialog.ShowDialog() != true) return false;
        var queue = dialog.PrintQueue;
        if (queue == null || IsFileQueue(queue.Name, queue.QueuePort?.Name))
        {
            MessageBox.Show("Dosya oluşturmak için programın PDF dışa aktarma seçeneğini kullanın.", "Yazdırma");
            return false;
        }
        return true;
    }

    private static bool IsFileQueue(string name, string? port) =>
        port is null || port.StartsWith("FILE", StringComparison.OrdinalIgnoreCase) ||
        port.StartsWith("PORTPROMPT", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("PDF", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("XPS", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("OneNote", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Fax", StringComparison.OrdinalIgnoreCase);
}