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

    public async Task<string> GetPlaybackUrlAsync(int mediaId, string mediaType)
    {
        string? filePath = null;

        if (mediaType.ToLower() == "movie")
        {
            var movie = await _context.Movies
                .Include(m => m.MediaFile)
                .FirstOrDefaultAsync(m => m.Id == mediaId);
            filePath = movie?.MediaFile?.FilePath;
        }
        else if (mediaType.ToLower() == "episode")
        {
            var episode = await _context.Episodes
                .Include(e => e.MediaFile)
                .FirstOrDefaultAsync(e => e.Id == mediaId);
            filePath = episode?.MediaFile?.FilePath;
        }

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Media file not found");

        // Return URL to media controller
        return $"/api/media/file?path={Uri.EscapeDataString(filePath)}";
    }

    public async Task<PlaybackProgress?> GetPlaybackProgressAsync(string userId, int? movieId, int? episodeId)
    {
        return await _context.PlaybackProgresses
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                (movieId.HasValue ? p.MovieId == movieId : false) ||
                (episodeId.HasValue ? p.EpisodeId == episodeId : false));
    }

    public async Task UpdatePlaybackProgressAsync(string userId, int? movieId, int? episodeId, double position, double? duration)
    {
        var progress = await _context.PlaybackProgresses
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                (movieId.HasValue ? p.MovieId == movieId : false) ||
                (episodeId.HasValue ? p.EpisodeId == episodeId : false));

        if (progress == null)
        {
            progress = new PlaybackProgress
            {
                UserId = userId,
                MovieId = movieId,
                EpisodeId = episodeId,
                Position = position,
                Duration = duration
            };
            _context.PlaybackProgresses.Add(progress);
        }
        else
        {
            progress.Position = position;
            progress.Duration = duration;
            progress.LastPlayed = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsCompletedAsync(string userId, int? movieId, int? episodeId)
    {
        var progress = await _context.PlaybackProgresses
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                (movieId.HasValue ? p.MovieId == movieId : false) ||
                (episodeId.HasValue ? p.EpisodeId == episodeId : false));

        if (progress != null)
        {
            progress.IsCompleted = true;
            progress.LastPlayed = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
