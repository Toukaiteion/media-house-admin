namespace MediaHouse.Entities;

public enum RoleType
{
    Director,
    Actor,
    Writer
}

public class Staff
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<MediaStaff> MediaStaffs { get; set; } = [];
}
