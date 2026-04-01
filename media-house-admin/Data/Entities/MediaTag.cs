namespace MediaHouse.Data.Entities;

public class MediaTag
{
    public int MediaLibraryId { get; set; }
    public MediaType MediaType { get; set; }
    public int MediaId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public MediaLibrary MediaLibrary { get; set; } = null!;
}
