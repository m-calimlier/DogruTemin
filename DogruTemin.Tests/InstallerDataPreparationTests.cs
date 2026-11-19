using System.Diagnostics;
using System.Text;

namespace DogruTemin.Tests;

public sealed class InstallerDataPreparationTests : IDisposable
{
    private readonly List<string> _junctions = new();
    private readonly string _sandbox = Path.Combine(Path.GetTempPath(), "DogruTeminInstallerTests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void RedirectedDesktopWithTurkishAndBracketedPathCanBePreparedTwice()
    {
        var desktop = Path.Combine(_sandbox, "Masaüstü [kurum]");
        var physical = Path.Combine(_sandbox, "Redirected");
        _junctions.Add(desktop);
        Directory.CreateDirectory(physical);
        Assert.Equal(0, PowerShell($"New-Item -ItemType Junction -Path {Quote(desktop)} -Target {Quote(physical)} | Out-Null").Code);

        Assert.Equal(0, Prepare(desktop).Code);
        File.WriteAllText(Path.Combine(physical, "DoğruTemin", "Veri", "kayit.json"), "korunacak");
        Assert.Equal(0, Prepare(desktop).Code);
        Assert.Equal("korunacak", File.ReadAllText(Path.Combine(physical, "DoğruTemin", "Veri", "kayit.json")));
    }

    [Fact]
    public void JunctionInsideDataTreeCannotBeCleaned()
    {
        var desktop = Path.Combine(_sandbox, "Desktop");
        var outside = Path.Combine(_sandbox, "Outside");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "record.txt"), "korunacak");
        Directory.CreateDirectory(Path.Combine(desktop, "DoğruTemin"));
        var link = Path.Combine(desktop, "DoğruTemin", "Veri");
        _junctions.Add(link);
        Assert.Equal(0, PowerShell($"New-Item -ItemType Junction -Path {Quote(link)} -Target {Quote(outside)} | Out-Null").Code);

        var result = Prepare(desktop, clean: true);

        Assert.NotEqual(0, result.Code);
        Assert.Equal("korunacak", File.ReadAllText(Path.Combine(outside, "record.txt")));
        Assert.Contains("Bağlantılı", File.ReadAllText(Path.Combine(_sandbox, "diagnostic.log")), StringComparison.Ordinal);
    }

    [Fact]
    public void LegacyMigrationPreservesConflictingRecordsAndIsRetryable()
    {
        var desktop = Path.Combine(_sandbox, "Desktop");
        var legacy = Path.Combine(_sandbox, "Legacy");
        Directory.CreateDirectory(Path.Combine(legacy, "Veri"));
        Assert.Equal(0, Prepare(desktop).Code);
        var current = Path.Combine(desktop, "DoğruTemin", "Veri", "record.json");
        File.WriteAllText(current, "guncel");
        File.WriteAllText(Path.Combine(legacy, "Veri", "record.json"), "eski");

        var migration = Prepare(desktop, legacy);
        Assert.True(migration.Code == 0, migration.Output);
        Assert.Equal("guncel", File.ReadAllText(current));
        var backup = Directory.GetFiles(Path.Combine(desktop, "DoğruTemin", "Yedekler"), "*", SearchOption.AllDirectories);
        Assert.Single(backup);
        Assert.Equal("eski", File.ReadAllText(backup[0]));
        var retry = Prepare(desktop, legacy);
        Assert.True(retry.Code == 0, retry.Output);
    }

    [Fact]
    public void LockedDataDirectoryProducesDiagnosticAndSucceedsAfterUnlock()
    {
        var desktop = Path.Combine(_sandbox, "Desktop");
        Assert.Equal(0, Prepare(desktop).Code);
        var marker = Path.Combine(desktop, "DoğruTemin", ".kurulum.lock");
        using (var handle = new FileStream(marker, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        {
            Assert.NotEqual(0, Prepare(desktop).Code);
            Assert.True(File.Exists(Path.Combine(_sandbox, "diagnostic.log")));
        }
        Assert.Equal(0, Prepare(desktop).Code);
    }

    [Fact]
    public void CleanOnlyRemovesManagedDataFolders()
    {
        var desktop = Path.Combine(_sandbox, "Desktop");
        Assert.Equal(0, Prepare(desktop).Code);
        var root = Path.Combine(desktop, "DoğruTemin");
        File.WriteAllText(Path.Combine(root, "Veri", "old.json"), "eski");
        File.WriteAllText(Path.Combine(root, "notes.txt"), "korunacak");

        Assert.Equal(0, Prepare(desktop, clean: true).Code);

        Assert.False(File.Exists(Path.Combine(root, "Veri", "old.json")));
        Assert.Equal("korunacak", File.ReadAllText(Path.Combine(root, "notes.txt")));
    }

    private (int Code, string Output) Prepare(string desktop, string? legacy = null, bool clean = false)
    {
        Directory.CreateDirectory(_sandbox);
        legacy ??= Path.Combine(_sandbox, "NoLegacy");
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "installer", "Prepare-DesktopData.ps1")))
            folder = folder.Parent;
        Assert.NotNull(folder);
        var script = Path.Combine(folder!.FullName, "installer", "Prepare-DesktopData.ps1");
        return PowerShell($"& {Quote(script)} -Desktop {Quote(desktop)} -LegacyRoots {Quote(legacy)} -DiagnosticLog {Quote(Path.Combine(_sandbox, "diagnostic.log"))}" + (clean ? " -Clean" : ""));
    }

    private static string Quote(string text) => "'" + text.Replace("'", "''", StringComparison.Ordinal) + "'";

    private static (int Code, string Output) PowerShell(string command)
    {
        var start = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell", "v1.0", "powershell.exe"))
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var arg in new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-EncodedCommand",
            Convert.ToBase64String(Encoding.Unicode.GetBytes("$ErrorActionPreference='Stop'; " + command)) })
            start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        Assert.True(process.WaitForExit(60000), "Windows PowerShell zamanında tamamlanmadı.");
        return (process.ExitCode, output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult());
    }

    public void Dispose()
    {
        foreach (var link in _junctions.Distinct())
            if (Directory.Exists(link)) Directory.Delete(link, recursive: false);
        if (Directory.Exists(_sandbox))
            Directory.Delete(_sandbox, recursive: true);
    }
}
