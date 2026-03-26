namespace MediaHouse.Entities;

public class Season
{
    public int Id { get; set; }
    public int SeasonNumber { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public int TVShowId { get; set; }

    // Navigation properties
    public TVShow TVShow { get; set; } = null!;
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
