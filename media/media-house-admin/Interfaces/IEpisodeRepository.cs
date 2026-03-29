using MediaHouse.Entities;

namespace MediaHouse.Interfaces;

public interface IEpisodeRepository : : IRepository<Episode>
{
    Task<List<Episode>> GetBySeasonAsync(string seasonId);
    Task<List<Episode>> GetByTVShowAsync(string tvShowId);
    Task<Episode?> GetByNumberAsync(string seasonId, int episodeNumber);
}
