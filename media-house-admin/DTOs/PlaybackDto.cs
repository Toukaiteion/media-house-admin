namespace MediaHouse.DTOs;

public class PlaybackUrlDto
{
    public string Url { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public bool CanDirectPlay { get; set; }
}

public class PlaybackProgressDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }
    public double Position { get; set; }
    public double? Duration { get; set; }
    public DateTime LastPlayed { get; set; }
    public bool IsCompleted { get; set; }
}

public class UpdatePlaybackProgressDto
{
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }
    public double Position { get; set; }
    public double? Duration { get; set; }
}
