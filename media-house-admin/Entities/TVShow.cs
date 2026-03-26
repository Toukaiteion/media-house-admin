namespace MediaHouse.Entities;

public class TVShow
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? PosterPath { get; set; }
    public string? FanartPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? Overview { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public int MediaLibraryId { get; set; }

    // Navigation properties
    public MediaLibrary MediaLibrary { get; set; } = null!;
    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    public NfoMetadata? Metadata { get; set; }
}
