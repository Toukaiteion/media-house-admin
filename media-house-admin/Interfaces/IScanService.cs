using MediaHouse.Data.Entities;

namespace MediaHouse.Interfaces;

public interface IScanService
{
    Task<SystemSyncLog> StartFullScanAsync(string libraryId);
    Task<SystemSyncLog> StartIncrementalScanAsync(string libraryId);
    Task<SystemSyncLog?> GetLatestScanLogAsync(string libraryId);
    Task<List<SystemSyncLog>> GetScanLogsAsync(string libraryId, int limit = 10);
}
