namespace MediaHouse.Entities;

public class Episode
{
    public int Id { get; set; }
    public int EpisodeNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Overview { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public int SeasonId { get; set; }

    // Navigation properties
    public Season Season { get; set; } = null!;
    public MediaFile? MediaFile { get; set; }
    public NfoMetadata? Metadata { get; set; }
}
