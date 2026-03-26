using MediaHouse.Entities;

namespace MediaHouse.Interfaces;

public interface IMetadataService
{
    Task<NfoMetadata?> ParseNfoFileAsync(string filePath);
    Task<NfoMetadata?> GetMetadataAsync(int movieId = 0, int tvShowId = 0, int episodeId = 0);
    Task<bool> UpdateMetadataAsync(NfoMetadata metadata);
    Task<string?> ExtractImageAsync(string mediaPath, string imageType); // poster, fanart, thumb
}
