using MediaHouse.Entities;

namespace MediaHouse.Interfaces;

public interface ISeasonRepository : IRepository<Season>
{
    Task<List<Season>> GetByTVShowAsync(string tvShowId);
    Task<Season?> GetByNumberAsync(string tvShowId, int seasonNumber);
}
