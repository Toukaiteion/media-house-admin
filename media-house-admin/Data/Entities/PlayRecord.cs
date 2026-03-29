namespace MediaHouse.Data.Entities;

public class PlayRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string MediaLibraryId { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string MediaId { get; set; } = string.Empty;                         // movies id / episode id
    public long PositionMs { get; set; } = 0;                // 播放进度（毫秒）
    public bool IsFinished { get; set; } = false;
    public DateTime? LastPlayTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}
