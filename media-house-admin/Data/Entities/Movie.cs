namespace MediaHouse.Data.Entities;

public class Movie
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MediaLibraryId { get; set; } = string.Empty;
    public string? Num { get; set; }                        // 编号/排序号
    public string Title { get; set; } = string.Empty;        // 标题
    public string? OriginalTitle { get; set; }                // 原始标题
    public string? Studio { get; set; }                      // 制片公司/工作室
    public string? Maker { get; set; }                       // 制作商
    public string? ReleaseDate { get; set; }                // 上映日期
    public int? Runtime { get; set; }                       // 时长(分钟)
    public decimal? Rating { get; set; }                     // 评分
    public string? Overview { get; set; }                     // 简介
    public string? Description { get; set; }                  // 详细描述
    public string? PosterPath { get; set; }                  // 海报
    public string? ThumbPath { get; set; }                   // 缩略图
    public string? FanartPath { get; set; }                  // 粉丝图
    public string? BackdropPath { get; set; }                 // 背景图
    public string? ScreenshotsPath { get; set; }             // 截图路径
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public MediaLibrary MediaLibrary { get; set; } = null!;
    public MediaFile? MediaFile { get; set; }
    public NfoMetadata? Metadata { get; set; }
}
