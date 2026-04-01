using MediaHouse.Data.Entities;

namespace MediaHouse.Interfaces;

public interface ITVShowRepository : IRepository<TVShow>
{
    Task<List<TVShow>> GetByLibraryAsync(int libraryId);
    Task<TVShow?> GetByTitleAsync(string title);
    Task<List<TVShow>> SearchAsync(string query);
}
