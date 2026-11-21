using DogruTemin.Core.Models;
using DogruTemin.Core.Modules.Archive;
using DogruTemin.Core.Modules.Records;
using DogruTemin.Core.Modules.Session;
using DogruTemin.Core.Services;

namespace DogruTemin.Tests;

public sealed class FoundationModuleTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void AnyPendingEditorPreventsTheSessionFromAppearingSaved(int editor)
    {
        var session = new ProcurementSession();
        if (editor == 0) session.RecordDirty = true;
        if (editor == 1) session.FirmEditorDirty = true;
        if (editor == 2) session.ProductEditorDirty = true;
        if (editor == 3) session.OfferEditorDirty = true;
        session.MarkSaved(new DateTime(2026, 11, 21));
        Assert.Equal(editor != 0, session.HasUnsavedWork);
    }

    [Fact]
    public void LoadingAnotherRecordClearsEditorsAndArchiveEditAuthority()
    {
        var session = new ProcurementSession();
        session.LoadMetadata(new ProcurementData { RecordId = "first", Revision = 3 });
        session.BeginArchiveEdit("first");
        session.RecordDirty = session.ProductEditorDirty = true;
        session.LoadMetadata(new ProcurementData { RecordId = "second", Revision = 5 });
        Assert.Equal("second", session.RecordId);
        Assert.Equal(5, session.Revision);
        Assert.False(session.IsEditingArchiveRecord);
        Assert.False(session.HasUnsavedWork);
        Assert.Throws<InvalidOperationException>(() => session.BeginArchiveEdit("first"));
    }

    [Fact]
    public void ArchiveSummaryCalculatesFromRecordsAndSnapshotsReadWarnings()
    {
        var store = new MemoryRecordStore();
        var data = ProcurementBoundaryTests.Scenario(2);
        data.RecordId = "active";
        data.Products.Add(new ProductItem { Name = "" });
        store.Records.Add(data);
        store.Warnings.Add("bozuk kayıt");
        var listing = new ArchiveQueryService(store).Read("active");
        var row = Assert.Single(listing.Records);
        Assert.True(row.IsActive);
        Assert.Equal(2, row.ProductCount);
        Assert.Equal(1, row.FirmCount);
        Assert.Equal(2m, row.FinalTotal);
        store.Warnings.Clear();
        Assert.Equal("bozuk kayıt", Assert.Single(listing.Warnings));
    }

    [Fact]
    public void SaveWorkflowUsesTheRecordContractWithoutFilesystemAccess()
    {
        var store = new MemoryRecordStore();
        var data = ProcurementBoundaryTests.Scenario(1);
        data.RecordId = "new";
        var result = new ProcurementWorkflow(store).Save(data, false);
        Assert.True(result.SavedSuccessfully);
        Assert.Equal(1, store.SaveCalls);
    }

    private sealed class MemoryRecordStore : IProcurementRecordStore
    {
        public List<ProcurementData> Records { get; } = new();
        public List<string> Warnings { get; } = new();
        public int SaveCalls { get; private set; }
        public string? LastBackupWarning => null;
        public IReadOnlyList<string> LastArchiveWarnings => Warnings;
        public IReadOnlyList<ProcurementData> ListRecords() => Records;
        public ProcurementData? LoadRecord(string recordId, out string? warning)
        { warning = null; return Records.FirstOrDefault(record => record.RecordId == recordId); }
        public void Save(ProcurementData data, bool createBackup = true) { SaveCalls++; Records.Add(data); }
        public void ActivateRecord(ProcurementData data) => throw new NotSupportedException();
        public void ClearActiveFile() => throw new NotSupportedException();
        public bool DeleteRecord(string recordId) => Records.RemoveAll(record => record.RecordId == recordId) > 0;
        public ProcurementData? RestoreMostRecentlyDeleted(out string? warning) { warning = null; return null; }
    }
}
