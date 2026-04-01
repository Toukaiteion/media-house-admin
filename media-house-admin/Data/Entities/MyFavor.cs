namespace MediaHouse.Data.Entities;

public class MyFavor
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MediaLibraryId { get; set; }
    public MediaType MediaType { get; set; }          // movie / tv
    public int MediaId { get; set; }                 // movies id / tv_shows id
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AppUser User { get; set; } = null!;
    public MediaLibrary MediaLibrary { get; set; } = null!;
}
