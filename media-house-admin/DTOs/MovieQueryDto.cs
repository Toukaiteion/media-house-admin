namespace MediaHouse.DTOs;

public class MovieQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? LibraryId { get; set; }
    public string? Tags { get; set; }          // 逗号分隔的标签
    public int? ActorId { get; set; }         // 演员ID
    public int? UserId { get; set; }           // 用户ID（用于 recent 和 favor 筛选）
    public string? Filter { get; set; }        // 筛选类型: tags, actor, recent, mostly_play, favor
}
