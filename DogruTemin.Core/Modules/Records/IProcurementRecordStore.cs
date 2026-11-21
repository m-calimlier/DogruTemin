using DogruTemin.Core.Models;

namespace DogruTemin.Core.Modules.Records;

/// <summary>Record operations used by application workflows; no filesystem or UI types.</summary>
public interface IProcurementRecordStore
{
    string? LastBackupWarning { get; }
    IReadOnlyList<string> LastArchiveWarnings { get; }
    IReadOnlyList<ProcurementData> ListRecords();
    ProcurementData? LoadRecord(string recordId, out string? warning);
    void Save(ProcurementData data, bool createBackup = true);
    void ActivateRecord(ProcurementData data);
    void ClearActiveFile();
    bool DeleteRecord(string recordId);
    ProcurementData? RestoreMostRecentlyDeleted(out string? warning);
}
