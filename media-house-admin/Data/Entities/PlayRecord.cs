namespace MediaHouse.Data.Entities;

public class PlayRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MediaLibraryId { get; set; }
    public MediaType MediaType { get; set; }
    public int MediaId { get; set; } = 0;                         // movies id / episode id
    public long PositionMs { get; set; } = 0;                // 播放进度（毫秒）
    public bool IsFinished { get; set; } = false;
    public DateTime? LastPlayTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}
