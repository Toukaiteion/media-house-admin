using MediaHouse.Entities;

namespace MediaHouse.Interfaces;

public interface ISeasonRepository : IRepository<Season>
{
    Task<List<Season>> GetByTVShowAsync(int tvShowId);
    Task<Season?> GetByNumberAsync(int tvShowId, int seasonNumber);
}
