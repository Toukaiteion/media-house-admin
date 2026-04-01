namespace MediaHouse.Data.Entities;

public enum MediaStaffType
{
    Movie,
    TvShow,
    Season,
    Episode
}

public class MediaStaff
{
    public int Id { get; set; }
    public MediaStaffType MediaType { get; set; }    // movie / tv_show / season / episode
    public int MediaId { get; set; }             // 对应媒体ID
    public int StaffId { get; set; }               // 关联人员ID
    public RoleType RoleType { get; set; }          // director / actor / writer
    public string? RoleName { get; set; }           // 饰演角色名（演员专用）
    public int SortOrder { get; set; } = 0;         // 排序（主演靠前）
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Staff Staff { get; set; } = null!;
}
