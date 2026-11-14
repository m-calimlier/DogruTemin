using System;
using System.IO;
using System.Linq;
using System.Windows;
using DesktopWorkspace;

namespace DogruTemin.Wpf;

internal sealed class WorkspaceOpenFileDialog
{
    private readonly Microsoft.Win32.OpenFileDialog dialog = new();
    public string Title { get => dialog.Title; set => dialog.Title = value; }
    public string Filter { get => dialog.Filter; set => dialog.Filter = value; }
    public string InitialDirectory { get => dialog.InitialDirectory; set => dialog.InitialDirectory = value; }
    public bool Multiselect { get => dialog.Multiselect; set => dialog.Multiselect = value; }
    public bool CheckFileExists { get => dialog.CheckFileExists; set => dialog.CheckFileExists = value; }
    public string FileName => dialog.FileName;
    public string[] FileNames => dialog.FileNames;

    public bool? ShowDialog(Window? owner = null)
    {
        var root = DogruTemin.Core.Services.StorageService.ResolveDesktopRoot(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        if (string.IsNullOrEmpty(InitialDirectory) || !WorkspaceOutputPath.IsAllowed(root, Path.Combine(InitialDirectory, "probe.tmp")))
            InitialDirectory = root;
        dialog.FileOk += (_, args) =>
        {
            if (FileNames.All(file => WorkspaceOutputPath.IsAllowed(root, file))) return;
            args.Cancel = true;
            MessageBox.Show("Önce dosyayı masaüstündeki program klasörüne kopyalayın:\n" + root, "Dosya konumu");
        };
        return owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
    }
}