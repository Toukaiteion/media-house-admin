namespace MediaHouse.Data.Entities;

public class NfoMetadata
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? OriginalXml { get; set; }

    // Parsed fields
    public string? Title { get; set; }
    public string? Plot { get; set; }
    public string? Director { get; set; }
    public string? Writer { get; set; }
    public float? Rating { get; set; }
    public int? Year { get; set; }
    public string? Premiered { get; set; }
    public DateTime? DateAdded { get; set; }
    public string? Genre { get; set; }
    public string? Tags { get; set; }
    public string? Studios { get; set; }

    // Foreign keys
    public string? MovieId { get; set; }
    public string? TVShowId { get; set; }
    public string? EpisodeId { get; set; }

    // Navigation properties
    public Movie? Movie { get; set; }
    public TVShow? TVShow { get; set; }
    public Episode? Episode { get; set; }
}
