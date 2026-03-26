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
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LibraryType Type { get; set; }
    public string Path { get; set; } = string.Empty;
    public ScanStatus Status { get; set; } = ScanStatus.Idle;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEnabled { get; set; } = true;

    // Navigation properties
    public ICollection<Movie> Movies { get; set; } = [];
    public ICollection<TVShow> TVShows { get; set; } = [];
    public bool IsDeleted { get; internal set; }

}
