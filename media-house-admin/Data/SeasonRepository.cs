using MediaHouse.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Data;

public class SeasonRepository : Repository<Season>, Interfaces.ISeasonRepository
{
    public SeasonRepository(MediaHouseDbContext context, ILogger<SeasonRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<List<Season>> GetByTVShowAsync(int tvShowId)
    {
        return await _dbSet
            .Include(s => s.Episodes)
            .Where(s => s.TVShowId == tvShowId && !s.IsDeleted)
            .OrderBy(s => s.SeasonNumber)
            .ToListAsync();
    }

    public async Task<Season?> GetByNumberAsync(int tvShowId, int seasonNumber)
    {
        return await _dbSet
            .Include(s => s.Episodes)
            .FirstOrDefaultAsync(s => s.TVShowId == tvShowId && s.SeasonNumber == seasonNumber && !s.IsDeleted);
    }
}
