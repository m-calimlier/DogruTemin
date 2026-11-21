using DogruTemin.Core.Models;
using DogruTemin.Core.Modules.Records;
using DogruTemin.Core.Services;

namespace DogruTemin.Core.Modules.Archive;

public sealed record ArchiveSummary(ProcurementData Record, bool IsActive, int ProductCount, int FirmCount,
    decimal FinalTotal, DateTime LastSavedAt);

public sealed record ArchiveListing(IReadOnlyList<ArchiveSummary> Records, IReadOnlyList<string> Warnings);

/// <summary>Builds archive read models without referencing a window or WPF controls.</summary>
public sealed class ArchiveQueryService(IProcurementRecordStore records)
{
    public ArchiveListing Read(string? activeRecordId)
    {
        var rows = records.ListRecords().Select(data => new ArchiveSummary(data,
            !string.IsNullOrWhiteSpace(activeRecordId) && data.RecordId == activeRecordId,
            data.Products.Count(product => !string.IsNullOrWhiteSpace(product.Name)),
            data.Firms.Count(firm => !string.IsNullOrWhiteSpace(firm.Name)),
            ProcurementEngine.Calculate(data).MarketResultTotal,
            data.LastSavedAt ?? data.CreatedAt)).ToArray();
        return new ArchiveListing(rows, records.LastArchiveWarnings.ToArray());
    }
}
