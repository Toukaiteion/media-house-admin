namespace MediaHouse.Data.Entities;

public class MyFavor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string MediaLibraryId { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }          // movie / tv
    public string MediaId { get; set; } = string.Empty;                 // movies id / tv_shows id
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AppUser User { get; set; } = null!;
    public MediaLibrary MediaLibrary { get; set; } = null!;
}
