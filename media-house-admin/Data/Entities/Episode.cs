namespace MediaHouse.Data.Entities;

public class Episode
{
    public int Id { get; set; }
    public int TVShowId { get; set; }
    public int SeasonId { get; set; }
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
}
