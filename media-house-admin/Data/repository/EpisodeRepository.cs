using MediaHouse.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Data.repository;

public class EpisodeRepository(MediaHouseDbContext context, ILogger<EpisodeRepository> logger)
    : Repository<Episode>(context, logger), Interfaces.IEpisodeRepository
{
    public async Task<List<Episode>> GetBySeasonAsync(int seasonId)
    {
        return await _dbSet
            .Include(e => e.MediaFile)
            .Include(e => e.Metadata)
            .Where(e => e.SeasonId == seasonId && !e.IsDeleted)
            .OrderBy(e => e.EpisodeNumber)
            .ToListAsync();
    }

    public async Task<List<Episode>> GetByTVShowAsync(int tvShowId)
    {
        return await _dbSet
            .Include(e => e.Season)
            .Include(e => e.MediaFile)
            .Where(e => e.TVShowId == tvShowId && !e.IsDeleted)
            .OrderBy(e => e.Season.SeasonNumber)
            .ThenBy(e => e.EpisodeNumber)
            .ToListAsync();
    }

    public async Task<Episode?> GetByNumberAsync(int seasonId, int episodeNumber)
    {
        return await _dbSet
            .Include(e => e.MediaFile)
            .Include(e => e.Metadata)
            .FirstOrDefaultAsync(e => e.SeasonId == seasonId && e.EpisodeNumber == episodeNumber && !e.IsDeleted);
    }
}
