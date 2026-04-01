using MediaHouse.Data.Entities;

namespace MediaHouse.Interfaces;

public interface IEpisodeRepository : IRepository<Episode>
{
    Task<List<Episode>> GetBySeasonAsync(int seasonId);
    Task<List<Episode>> GetByTVShowAsync(int tvShowId);
    Task<Episode?> GetByNumberAsync(int seasonId, int episodeNumber);
}
