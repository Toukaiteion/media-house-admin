namespace MediaHouse.Entities;

public class Season
{
    public int Id { get; set; }
    public int TVShowId { get; set; }
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
