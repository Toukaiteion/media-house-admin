using MediaHouse.Interfaces;
using System.Xml.Linq;

namespace MediaHouse.Services;

public record NfoParseResult(
    string Title,
    string OriginalXml,
    string? Summary,
    string? Studios,
    int? Year,
    string? Premiered,
    string? Genre,
    string? Tags,
    Dictionary<string, string> ImagePaths,
    List<string> Actors,
    int? Runtime,
    string? Num,
    string? Maker
);

public class MetadataService(ILogger<MetadataService> logger) : IMetadataService
{
    private readonly ILogger<MetadataService> _logger = logger;

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

            var OriginalXml = xml;
            var Title = GetElementValue(root, "title");
            var OriginalTitle = GetElementValue(root, "originaltitle");
            var summary = GetElementValue(root, "plot") ?? GetElementValue(root, "outline");
            var Studios = GetElementValue(root, "studio");
            var Year = ParseInt(GetElementValue(root, "year"));
            var Premiered = GetElementValue(root, "premiered") ?? GetElementValue(root, "releasedate") ?? GetElementValue(root, "release");
            var Genre = GetConcatenatedElementValues(root, "genre");
            var Tags = GetConcatenatedElementValues(root, "tag");

            var imagePaths = new Dictionary<string, string>();
            imagePaths["poster"] = GetElementValue(root, "poster") ?? "";
            imagePaths["thumb"] = GetElementValue(root, "thumb") ?? "";
            imagePaths["fanart"] = GetElementValue(root, "fanart") ?? "";

            var actors = GetElementValues(root, "actor", "name");
            var runtime = ParseInt(GetElementValue(root, "runtime"));
            var num = GetElementValue(root, "num");
            var maker = GetElementValue(root, "maker");

            return new NfoParseResult(
                Title ?? OriginalTitle ?? "Unknown",
                OriginalXml,
                summary,
                Studios,
                Year,
                Premiered,
                Genre,
                Tags,
                imagePaths,
                actors,
                runtime,
                num,
                maker
            );
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

    public async Task<string?> ExtractImageAsync(string mediaPath, string imageType)
    {
        // TODO: Implement image extraction from media file
        return null;
    }
}
