using MediaHouse.Entities;

namespace MediaHouse.Interfaces;

public interface IMetadataService
{
    Task<NfoMetadata?> ParseNfoFileAsync(string filePath);
    Task<NfoMetadata?> GetMetadataAsync(string movieId = "", string tvShowId = "", string episodeId = "");
    Task<bool> UpdateMetadataAsync(NfoMetadata metadata);
    Task<string?> ExtractImageAsync(string mediaPath, string imageType); // poster, fanart, thumb
}
