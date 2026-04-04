using MediaHouse.Data;
using MediaHouse.Data.Entities;
using MediaHouse.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Services;

public class TagService(MediaHouseDbContext context, ILogger<TagService> logger) : ITagService
{
    private readonly MediaHouseDbContext _context = context;
    private readonly ILogger<TagService> _logger = logger;

    public async Task<(List<Tag> Tags, int TotalCount)> GetTagsAsync(int page, int pageSize)
    {
        var query = _context.Tags.AsQueryable();
        var totalCount = await query.CountAsync();

        var tags = await query
            .OrderBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (tags, totalCount);
    }
}
