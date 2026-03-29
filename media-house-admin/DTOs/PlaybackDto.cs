using MediaHouse.Entities;

namespace MediaHouse.DTOs;

public class PlaybackUrlDto
{
    public string Url { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public bool CanDirectPlay { get; set; }
}

public class PlayRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string MediaLibraryId { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string MediaId { get; set; } = string.Empty;
    public long PositionMs { get; set; }
    public bool IsFinished { get; set; }
    public DateTime? LastPlayTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdatePlayRecordDto
{
    public string UserId { get; set; } = string.Empty;
    public string MediaLibraryId { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string MediaId { get; set; } = string.Empty;
    public double PositionSeconds { get; set; }
}
