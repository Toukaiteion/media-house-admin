namespace MediaHouse.Entities;

public enum LibraryType
{
    Movie,
    TVShow
}

public enum ScanStatus
{
    Idle,
    Scanning,
    Error
}

public class MediaLibrary
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public LibraryType Type { get; set; }
    public string Path { get; set; } = string.Empty;
    public ScanStatus Status { get; set; } = ScanStatus.Idle;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEnabled { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<Movie> Movies { get; set; } = [];
    public ICollection<TVShow> TVShows { get; set; } = [];
}
