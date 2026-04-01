namespace MediaHouse.Data.Entities;

public enum MediaType
{
    Movie,
    Episode
}

public class MediaFile
{
    public int Id { get; set; }
    public MediaType MediaType { get; set; }
    public int MediaId { get; set; }                     // 对应 movies 或 episodes 的ID
    public string Path { get; set; } = string.Empty;      // 文件路径
    public string FileName { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public string? Container { get; set; }                  // mkv, mp4...
    public string? VideoCodec { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AudioCodec { get; set; }
    public long? SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys (legacy - kept for backward compatibility)
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }

    // Navigation properties
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}
