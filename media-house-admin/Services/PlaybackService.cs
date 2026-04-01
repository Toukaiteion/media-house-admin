using MediaHouse.Data.Entities;
using MediaHouse.Interfaces;
using MediaHouse.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Services;

public class PlayRecordService(MediaHouseDbContext context, ILogger<PlayRecordService> logger) : IPlayRecordService
{
    private readonly MediaHouseDbContext _context = context;
    private readonly ILogger<PlayRecordService> _logger = logger;

    public async Task<string> GetPlaybackUrlAsync(int mediaId, string mediaType)
    {
        string? filePath = null;

        if (mediaType.Equals("movie", StringComparison.CurrentCultureIgnoreCase))
        {
            var movie = await _context.Movies
                .Include(m => m.MediaFile)
                .FirstOrDefaultAsync(m => m.Id == mediaId);
            filePath = movie?.MediaFile?.Path;
        }
        else if (mediaType.Equals("episode", StringComparison.CurrentCultureIgnoreCase))
        {
            var episode = await _context.Episodes
                .Include(e => e.MediaFile)
                .FirstOrDefaultAsync(e => e.Id == mediaId);
            filePath = episode?.MediaFile?.Path;
        }

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Media file not found");

        // Return URL to media controller
        return $"/api/media/file?path={Uri.EscapeDataString(filePath)}";
    }

    public async Task<PlayRecord?> GetPlaybackProgressAsync(int userId, int mediaLibraryId, MediaType mediaType, int mediaId)
    {
        return await _context.PlayRecords
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.MediaLibraryId == mediaLibraryId &&
                p.MediaType == mediaType &&
                p.MediaId == mediaId);
    }

    public async Task UpdatePlaybackProgressAsync(int userId, int mediaLibraryId, MediaType mediaType, int mediaId, double positionSeconds)
    {
        var progress = await _context.PlayRecords
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.MediaLibraryId == mediaLibraryId &&
                p.MediaType == mediaType &&
                p.MediaId == mediaId);

        long positionMs = (long)(positionSeconds * 1000);

        if (progress == null)
        {
            progress = new PlayRecord
            {
                UserId = userId,
                MediaLibraryId = mediaLibraryId,
                MediaType = mediaType,
                MediaId = mediaId,
                PositionMs = positionMs,
                LastPlayTime = DateTime.UtcNow
            };
            _context.PlayRecords.Add(progress);
        }
        else
        {
            progress.PositionMs = positionMs;
            progress.LastPlayTime = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsCompletedAsync(int userId, int mediaLibraryId, MediaType mediaType, int mediaId)
    {
        var progress = await _context.PlayRecords
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.MediaLibraryId == mediaLibraryId &&
                p.MediaType == mediaType &&
                p.MediaId == mediaId);

        if (progress != null)
        {
            progress.IsFinished = true;
            progress.LastPlayTime = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
