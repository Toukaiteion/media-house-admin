namespace MediaHouse.Entities;

public class PlaybackProgress
{
    public int Id { get; set; }
    public string UserId { get; set; } = "default"; // MVP single user
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }
    public double Position { get; set; } // seconds
    public double? Duration { get; set; }
    public DateTime LastPlayed { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; } = false;

    // Navigation properties
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}
