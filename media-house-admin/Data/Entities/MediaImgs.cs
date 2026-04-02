namespace MediaHouse.Data.Entities;

public enum ImageType
{
    Poster,
    Thumb,
    Fanart,
}

public class MediaImgs
{
    public int Id { get; set; }
    public MediaType MediaType { get; set; }
    public int MediaId { get; set; }                     // 对应 movies 或 episodes 的ID
    public ImageType Type { get; set; }
    public string UrlName { get; set; } = string.Empty;   // 例如 p300111.jpg
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;      // 文件路径
    public string FileName { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? SizeBytes { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}
