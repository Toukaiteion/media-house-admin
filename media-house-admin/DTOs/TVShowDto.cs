namespace MediaHouse.DTOs;

public class TVShowDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? PosterPath { get; set; }
    public string? Overview { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MediaLibraryId { get; set; }
}

public class SeasonDto
{
    public int Id { get; set; }
    public int SeasonNumber { get; set; }
    public int TVShowId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EpisodeCount { get; set; }
}

public class EpisodeDto
{
    public int Id { get; set; }
    public int EpisodeNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Overview { get; set; }
    public int SeasonId { get; set; }

    // Media file info
    public string? FilePath { get; set; }
    public string? ContainerFormat { get; set; }
    public double? Duration { get; set; }
    public long? FileSize { get; set; }
}
