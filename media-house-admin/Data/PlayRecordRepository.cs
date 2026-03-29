using MediaHouse.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Data;

public class PlayRecordRepository(MediaHouseDbContext context, ILogger<PlayRecordRepository> logger)
    : Repository<PlayRecord>(context, logger), Interfaces.IPlayRecordRepository
{
    public async Task<PlayRecord?> GetByUserAndMediaAsync(string userId, MediaType mediaType, string mediaId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.MediaType == mediaType && pr.MediaId == mediaId);
    }

    public async Task<List<PlayRecord>> GetByUserAsync(string userId)
    {
        return await _dbSet
            .Where(pr => pr.UserId == userId)
            .OrderByDescending(pr => pr.LastPlayTime)
            .ToListAsync();
    }

    public async Task<List<PlayRecord>> GetRecentAsync(string userId, int limit = 10)
    {
        return await _dbSet
            .Where(pr => pr.UserId == userId && pr.LastPlayTime.HasValue)
            .OrderByDescending(pr => pr.LastPlayTime)
            .Take(limit)
            .ToListAsync();
    }
}
