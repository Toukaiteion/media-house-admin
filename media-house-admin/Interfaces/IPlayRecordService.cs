using MediaHouse.Data.Entities;

namespace MediaHouse.Interfaces;

public interface IPlayRecordService
{
    Task<string> GetPlaybackUrlAsync(int mediaId, string mediaType); // movie or episode
    Task<PlayRecord?> GetPlaybackProgressAsync(int userId, int mediaLibraryId, MediaType mediaType, int mediaId);
    Task UpdatePlaybackProgressAsync(int userId, int mediaLibraryId, MediaType mediaType, int mediaId, double positionSeconds);
    Task MarkAsCompletedAsync(int userId, int mediaLibraryId, MediaType mediaType, int mediaId);
}
