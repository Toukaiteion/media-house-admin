using System.ComponentModel.DataAnnotations.Schema;

namespace MediaHouse.Data.Entities;

public class MediaItem
{
    public int Id { get; set; }

    [ForeignKey("Library")]
    public int LibraryId { get; set; }
    public string Name { get; set; } = string.Empty;             // 媒体名
    public string Title { get; set; } = string.Empty;           // 标题
    public string? OriginalTitle { get; set; }                // 原始标题
    public string? Type { get; set; }                              // movie / tv (对应 type 字段)
    public string? ReleaseDate { get; set; }                // 上映日期
    public string? Summary { get; set; }                         // 简介
    public string? PosterPath { get; set; }                      // 海报
    public string? ThumbPath { get; set; }                       // 缩略图
    public string? FanartPath { get; set; }                      // 粉丝图
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
    // Navigation properties
    public MediaLibrary Library { get; set; } = null!;

    public Movie? Movie { get; set; }
}
