using DogruTemin.Core.Models;
using DogruTemin.Core.Services;
using DogruTemin.Wpf;

namespace DogruTemin.Tests;

public sealed class AutomaticOutputPathTests
{
    [Theory]
    [InlineData(".pdf", "Çıktılar")]
    [InlineData(".xlsx", "Çıktılar")]
    [InlineData(".json", "Veri")]
    [InlineData(".zip", "Yedekler")]
    public void ExportsChooseUniquePathsInsideTheirWorkspaceWithoutDialogs(string extension, string category)
    {
        var root = Path.Combine(Path.GetTempPath(), "DogruTemin_AutoExport_" + Guid.NewGuid().ToString("N"));
        try
        {
            var storage = new StorageService(root);
            var first = WorkspaceOutputFile.CreatePath(storage, "örnek" + extension, extension);
            File.WriteAllText(first, "önceki dosya");
            var second = WorkspaceOutputFile.CreatePath(storage, "örnek" + extension, extension);
            Assert.NotEqual(first, second);
            Assert.StartsWith(Path.Combine(root, category) + Path.DirectorySeparatorChar, first, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(extension, Path.GetExtension(first));
            Assert.True(Directory.Exists(Path.GetDirectoryName(first)));
            Assert.Equal("önceki dosya", File.ReadAllText(first));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void OfficialOutputsUseAStableDatedFolderUnderOutputs()
    {
        var root = Path.Combine(Path.GetTempPath(), "DogruTemin_DatedExport_" + Guid.NewGuid().ToString("N"));
        try
        {
            var storage = new StorageService(root);
            var data = new ProcurementData { RecordId = "deneme", JobTitle = "Deneme", CreatedAt = new DateTime(2026, 11, 20) };
            storage.Save(data, false);
            var folder = storage.OutputFolderFor(data.RecordId);
            Assert.Equal(Path.Combine(root, "Çıktılar", "2026-11-20", "İşlem_deneme"), folder);
            data.JobTitle = "Düzeltilmiş işlem adı";
            storage.Save(data, false);
            Assert.Equal(folder, storage.OutputFolderFor(data.RecordId));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
