namespace MediaHouse.DTOs;

public class PlayRecordUrlDto
{
    public string Url { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public bool CanDirectPlay { get; set; }
}

public class PlayRecordDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MediaLibraryId { get; set; }
    public int MediaId { get; set; }
    public long PositionMs { get; set; }
    public bool IsFinished { get; set; }
    public DateTime? LastPlayTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdatePlayRecordDto
{
    public int UserId { get; set; }
    public int MediaLibraryId { get; set; }
    public int MediaId { get; set; }
    public double PositionSeconds { get; set; }
}
