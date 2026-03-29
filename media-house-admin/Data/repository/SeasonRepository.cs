using MediaHouse.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Data.repository;

public class SeasonRepository(MediaHouseDbContext context, ILogger<SeasonRepository> logger)
    : Repository<Season>(context, logger), Interfaces.ISeasonRepository
{
    public async Task<List<Season>> GetByTVShowAsync(string tvShowId)
    {
        return await _dbSet
            .Include(s => s.Episodes)
            .Where(s => s.TVShowId == tvShowId && !s.IsDeleted)
            .OrderBy(s => s.SeasonNumber)
            .ToListAsync();
    }

    public async Task<Season?> GetByNumberAsync(string tvShowId, int seasonNumber)
    {
        return await _dbSet
            .Include(s => s.Episodes)
            .FirstOrDefaultAsync(s => s.TVShowId == tvShowId && s.SeasonNumber == seasonNumber && !s.IsDeleted);
    }
}
