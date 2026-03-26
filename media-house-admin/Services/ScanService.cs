using MediaHouse.Entities;
using MediaHouse.Interfaces;
using MediaHouse.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Services;

public class ScanService : IScanService
{
    private readonly MediaHouseDbContext _context;
    private readonly ILogger<ScanService> _logger;

    public ScanService(MediaHouseDbContext context, ILogger<ScanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SystemSyncLog> StartFullScanAsync(int libraryId)
    {
        var library = await _context.MediaLibraries.FindAsync(libraryId);
        if (library == null)
            throw new InvalidOperationException($"Library {libraryId} not found");

        var log = new SystemSyncLog
        {
            MediaLibraryId = libraryId,
            SyncType = SyncType.FullScan,
            Status = SyncStatus.Started,
            StartTime = DateTime.UtcNow
        };

        _context.SystemSyncLogs.Add(log);
        await _context.SaveChangesAsync();

        try
        {
            log.Status = SyncStatus.InProgress;
            await _context.SaveChangesAsync();

            // TODO: Implement full scan logic
            _logger.LogInformation("Starting full scan for library {LibraryId}", libraryId);

            await Task.Delay(1000); // Placeholder

            log.Status = SyncStatus.Completed;
            log.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full scan failed for library {LibraryId}", libraryId);
            log.Status = SyncStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            throw;
        }

        return log;
    }

    public async Task<SystemSyncLog> StartIncrementalScanAsync(int libraryId)
    {
        var library = await _context.MediaLibraries.FindAsync(libraryId);
        if (library == null)
            throw new InvalidOperationException($"Library {libraryId} not found");

        var log = new SystemSyncLog
        {
            MediaLibraryId = libraryId,
            SyncType = SyncType.IncrementalScan,
            Status = SyncStatus.Started,
            StartTime = DateTime.UtcNow
        };

        _context.SystemSyncLogs.Add(log);
        await _context.SaveChangesAsync();

        try
        {
            log.Status = SyncStatus.InProgress;
            await _context.SaveChangesAsync();

            // TODO: Implement incremental scan logic
            _logger.LogInformation("Starting incremental scan for library {LibraryId}", libraryId);

            log.Status = SyncStatus.Completed;
            log.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Incremental scan failed for library {LibraryId}", libraryId);
            log.Status = SyncStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            throw;
        }

        return log;
    }

    public async Task<SystemSyncLog?> GetLatestScanLogAsync(int libraryId)
    {
        return await _context.SystemSyncLogs
            .Where(sl => sl.MediaLibraryId == libraryId)
            .OrderByDescending(sl => sl.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SystemSyncLog>> GetScanLogsAsync(int libraryId, int limit = 10)
    {
        return await _context.SystemSyncLogs
            .Where(sl => sl.MediaLibraryId == libraryId)
            .OrderByDescending(sl => sl.StartTime)
            .Take(limit)
            .ToListAsync();
    }
}
