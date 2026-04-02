namespace MediaHouse.Data.Entities;

public class MediaTag
{
    public int MediaLibraryId { get; set; }
    public MediaType MediaType { get; set; }
    public int MediaId { get; set; }
    public int TagId { get; set; }             // Foreign key to tags.id
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public MediaLibrary MediaLibrary { get; set; } = null!;
    public Tag? Tag { get; set; }
}
