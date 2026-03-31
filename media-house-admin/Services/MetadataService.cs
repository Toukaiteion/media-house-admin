using MediaHouse.Data.Entities;
using MediaHouse.Interfaces;
using System.Xml.Linq;

namespace MediaHouse.Services;

public record NfoParseResult(
    NfoMetadata Metadata,
    Dictionary<string, string> ImagePaths,
    List<string> Actors,
    int? Runtime,
    string? Num,
    string? Maker
);

public class MetadataService(ILogger<MetadataService> logger) : IMetadataService
{
    private readonly ILogger<MetadataService> _logger = logger;

    public async Task<NfoMetadata?> ParseNfoFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            var xml = await File.ReadAllTextAsync(filePath);

            // TODO: Implement proper XML parsing
            var metadata = new NfoMetadata
            {
                OriginalXml = xml
            };

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse NFO file: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<NfoParseResult?> ParseNfoFileFullAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            var xml = await File.ReadAllTextAsync(filePath);
            var doc = XDocument.Parse(xml);
            var root = doc.Root;

            if (root == null)
                return null;

            var metadata = new NfoMetadata
            {
                OriginalXml = xml,
                Title = GetElementValue(root, "title"),
                Plot = GetElementValue(root, "plot") ?? GetElementValue(root, "outline"),
                Studios = GetElementValue(root, "studio"),
                Year = ParseInt(GetElementValue(root, "year")),
                Premiered = ParseDate(GetElementValue(root, "premiered") ?? GetElementValue(root, "releasedate") ?? GetElementValue(root, "release")),
                Genre = GetConcatenatedElementValues(root, "genre"),
                Tags = GetConcatenatedElementValues(root, "tag")
            };

            var imagePaths = new Dictionary<string, string>();
            imagePaths["poster"] = GetElementValue(root, "poster") ?? "";
            imagePaths["thumb"] = GetElementValue(root, "thumb") ?? "";
            imagePaths["fanart"] = GetElementValue(root, "fanart") ?? "";

            var actors = GetElementValues(root, "actor", "name");
            var runtime = ParseInt(GetElementValue(root, "runtime"));
            var num = GetElementValue(root, "num");
            var maker = GetElementValue(root, "maker");

            return new NfoParseResult(metadata, imagePaths, actors, runtime, num, maker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse NFO file: {FilePath}", filePath);
            return null;
        }
    }

    private string? GetElementValue(XElement parent, string elementName)
    {
        return parent.Element(elementName)?.Value?.Trim();
    }

    private List<string> GetElementValues(XElement parent, string elementName, string subElementName)
    {
        var values = parent.Elements(elementName)
            .Select(e => e.Element(subElementName)?.Value?.Trim())
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        return values;
    }

    private string GetConcatenatedElementValues(XElement parent, string elementName)
    {
        var values = parent.Elements(elementName)
            .Select(e => e.Value?.Trim())
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        return string.Join(",", values);
    }

    private int? ParseInt(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (int.TryParse(value, out var result))
            return result;

        return null;
    }

    private DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (DateTime.TryParse(value, out var result))
            return result;

        return null;
    }

    public async Task<NfoMetadata?> GetMetadataAsync(string movieId = "", string tvShowId = "", string episodeId = "")
    {
        // TODO: Implement with proper database queries
        return null;
    }

    public async Task<bool> UpdateMetadataAsync(NfoMetadata metadata)
    {
        // TODO: Implement
        return true;
    }

    public async Task<string?> ExtractImageAsync(string mediaPath, string imageType)
    {
        // TODO: Implement image extraction from media file
        return null;
    }
}
