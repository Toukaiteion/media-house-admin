namespace MediaHouse.Data.Entities;

public class Season
{
    public int Id { get; set; }
    public string TVShowId { get; set; } = string.Empty;
    public int SeasonNumber { get; set; }
    public string? Name { get; set; }
    public string? Overview { get; set; }
    public string? PosterPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public TVShow TVShow { get; set; } = null!;
    public ICollection<Episode> Episodes { get; set; } = [];
}
