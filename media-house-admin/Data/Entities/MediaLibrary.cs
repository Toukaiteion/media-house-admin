namespace MediaHouse.Data.Entities;

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
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
    public bool IsEnabled { get; set; } = true;

    public ICollection<MediaItem> MediaItems { get; set; } = [];
}
