namespace MediaHouse.Entities;

public class MediaTag
{
    public string MediaLibraryId { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string MediaId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public MediaLibrary MediaLibrary { get; set; } = null!;
}
