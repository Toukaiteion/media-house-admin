using MediaHouse.Data.Entities;

namespace MediaHouse.Interfaces;

public interface IMediaFileService
{
    Task<MediaFile?> GetMediaFileByPathAsync(string path);
    Task<MediaFile?> GetMediaFileByIdAsync(string id);
    Task<MediaFile> CreateMediaFileAsync(string filePath, string? movieId = null, string? episodeId = null);
    Task<MediaFile?> UpdateMediaFileAsync(string id, MediaFile updatedFile);
    Task<bool> DeleteMediaFileAsync(string id);
    Task<List<MediaFile>> GetMediaFilesForLibraryAsync(string libraryId);
}
