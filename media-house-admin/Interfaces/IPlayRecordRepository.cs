using MediaHouse.Entities;

namespace MediaHouse.Interfaces;

public interface IPlayRecordRepository : IRepository<PlayRecord>
{
    Task<PlayRecord?> GetByUserAndMediaAsync(string userId, MediaType mediaType, string mediaId);
    Task<List<PlayRecord>> GetByUserAsync(string userId);
    Task<List<PlayRecord>> GetRecentAsync(string userId, int limit = 10);
}
