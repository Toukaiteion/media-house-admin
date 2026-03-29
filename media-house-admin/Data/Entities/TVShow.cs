namespace MediaHouse.Data.Entities;

public class TVShow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MediaLibraryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Overview { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? PosterPath { get; set; }
    public string? BackdropPath { get; set; }
    public decimal? Rating { get; set; }
    public int TotalSeasons { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public MediaLibrary MediaLibrary { get; set; } = null!;
    public ICollection<Season> Seasons { get; set; } = [];
    public NfoMetadata? Metadata { get; set; }
}
