using DogruTemin.Core.Models;

namespace DogruTemin.Core.Modules.Session;

/// <summary>The active work session, independent of views and persistence.</summary>
public sealed class ProcurementSession
{
    public string RecordId { get; set; } = string.Empty;
    public string OperationNumber { get; set; } = string.Empty;
    public int Revision { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastSavedAt { get; set; }
    public SavedOutputInfo? LastOutput { get; set; }
    public string PreferredWinnerFirmId { get; set; } = string.Empty;
    public string? ArchiveEditRecordId { get; set; }
    public bool RecordDirty { get; set; }
    public bool FirmEditorDirty { get; set; }
    public bool ProductEditorDirty { get; set; }
    public bool OfferEditorDirty { get; set; }
    public bool HasUnsavedWork => RecordDirty || FirmEditorDirty || ProductEditorDirty || OfferEditorDirty;
    public bool IsEditingArchiveRecord => !string.IsNullOrWhiteSpace(ArchiveEditRecordId) && ArchiveEditRecordId == RecordId;

    public void LoadMetadata(ProcurementData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        RecordId = data.RecordId ?? string.Empty;
        OperationNumber = data.OperationNumber ?? string.Empty;
        Revision = data.Revision;
        CreatedAt = data.CreatedAt == default ? DateTime.Now : data.CreatedAt;
        LastSavedAt = data.LastSavedAt;
        LastOutput = data.LastOutput;
        PreferredWinnerFirmId = data.PreferredWinnerFirmId ?? string.Empty;
        ArchiveEditRecordId = null;
        RecordDirty = FirmEditorDirty = ProductEditorDirty = OfferEditorDirty = false;
    }

    public void BeginArchiveEdit(string recordId)
    {
        if (string.IsNullOrWhiteSpace(recordId) || recordId != RecordId)
            throw new InvalidOperationException("Yalnız yüklenmiş kayıt düzenlenebilir.");
        ArchiveEditRecordId = recordId;
    }

    public void MarkSaved(DateTime savedAt)
    {
        LastSavedAt = savedAt;
        RecordDirty = false;
    }
}
