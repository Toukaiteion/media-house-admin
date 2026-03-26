namespace MediaHouse.Entities;

public enum MediaFileType
{
    Video,
    Audio,
    Image,
    Subtitle
}

public class MediaFile
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContainerFormat { get; set; } = string.Empty;
    public string? VideoCodec { get; set; }
    public string? AudioCodec { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? Duration { get; set; }
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys (can be null for general media files)
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }

    // Navigation properties
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}
