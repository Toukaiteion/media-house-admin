using MediaHouse.Entities;

namespace MediaHouse.Interfaces;

public interface IPlayRecordService
{
    Task<string> GetPlaybackUrlAsync(string mediaId, string mediaType); // movie or episode
    Task<PlayRecord?> GetPlaybackProgressAsync(string userId, string mediaLibraryId, MediaType mediaType, string mediaId);
    Task UpdatePlaybackProgressAsync(string userId, string mediaLibraryId, MediaType mediaType, string mediaId, double positionSeconds);
    Task MarkAsCompletedAsync(string userId, string mediaLibraryId, MediaType mediaType, string mediaId);
}
