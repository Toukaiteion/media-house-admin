namespace MediaHouse.DTOs;

public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? PosterPath { get; set; }
    public string? Overview { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MediaLibraryId { get; set; }

    // Media file info
    public string? FilePath { get; set; }
    public string? ContainerFormat { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? Duration { get; set; }
    public long? FileSize { get; set; }
}
