using MediaHouse.Entities;

namespace MediaHouse.Interfaces;

public interface IPlaybackService
{
    Task<string> GetPlaybackUrlAsync(int mediaId, string mediaType); // movie or episode
    Task<PlaybackProgress?> GetPlaybackProgressAsync(string userId, int? movieId, int? episodeId);
    Task UpdatePlaybackProgressAsync(string userId, int? movieId, int? episodeId, double position, double? duration);
    Task MarkAsCompletedAsync(string userId, int? movieId, int? episodeId);
}
