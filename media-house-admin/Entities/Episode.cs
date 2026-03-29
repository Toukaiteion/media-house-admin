namespace MediaHouse.Entities;

public class Episode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TVShowId { get; set; } = string.Empty;
    public string SeasonId { get; set; } = string.Empty;
    public int EpisodeNumber { get; set; }
    public string? Title { get; set; }
    public string? Overview { get; set; }
    public int? Runtime { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public TVShow TVShow { get; set; } = null!;
    public Season Season { get; set; } = null!;
    public MediaFile? MediaFile { get; set; }
    public NfoMetadata? Metadata { get; set; }
}
