using MediaHouse.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Data;

public class PlayRecordRepository : Repository<PlayRecord>, Interfaces.IPlayRecordRepository
{
    public PlayRecordRepository(MediaHouseDbContext context, ILogger<PlayRecordRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<PlayRecord?> GetByUserAndMediaAsync(int userId, MediaType mediaType, int mediaId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.MediaType == mediaType && pr.MediaId == mediaId);
    }

    public async Task<List<PlayRecord>> GetByUserAsync(int userId)
    {
        return await _dbSet
            .Where(pr => pr.UserId == userId)
            .OrderByDescending(pr => pr.LastPlayTime)
            .ToListAsync();
    }

    public async Task<List<PlayRecord>> GetRecentAsync(int userId, int limit = 10)
    {
        return await _dbSet
            .Where(pr => pr.UserId == userId && pr.LastPlayTime.HasValue)
            .OrderByDescending(pr => pr.LastPlayTime)
            .Take(limit)
            .ToListAsync();
    }
}
