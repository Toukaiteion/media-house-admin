using MediaHouse.Entities;
using MediaHouse.Interfaces;
using MediaHouse.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Services;

public class PlaybackService : IPlaybackService
{
    private readonly MediaHouseDbContext _context;
    private readonly ILogger<PlaybackService> _logger;

    public PlaybackService(MediaHouseDbContext context, ILogger<PlaybackService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GetPlaybackUrlAsync(string mediaId, string mediaType)
    {
        string? filePath = null;

        if (mediaType.ToLower() == "movie")
        {
            var movie = await _context.Movies
                .Include(m => m.MediaFile)
                .FirstOrDefaultFirstOrDefaultAsync(m => m.Id == mediaId);
            filePath = movie?.MediaFile?.Path;
        }
        else if (mediaType.ToLower() == "episode")
        {
            var episode = await _context.Episodes
                .Include(e => e.MediaFile)
                .FirstOrDefaultAsync(e => => e.Id == mediaId);
            filePath = episode?.MediaFile?.Path;
        }

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Media file not found");

        // Return URL to media controller
        return $"/api/media/file?path={Uri.EscapeDataString(filePath)}";
    }

    public async Task<PlayRecord?> GetPlaybackProgressAsync(string userId, string mediaLibraryId, MediaType mediaType, string mediaId)
    {
        return await _context.PlayRecords
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.MediaLibraryId == mediaLibraryId &&
                p.MediaType == mediaType &&
                p.MediaId == mediaId);
    }

    public async Task UpdatePlaybackProgressAsync(string userId, string mediaLibraryId, MediaType mediaType, string mediaId, double positionSeconds)
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

    public async Task MarkAsCompletedAsync(string userId, string mediaLibraryId, MediaType mediaType, string mediaId)
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
