using MediaHouse.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Data;

public class TVShowRepository : Repository<TVShow>, Interfaces.ITVShowRepository
{
    public TVShowRepository(MediaHouseDbContext context, ILogger<TVShowRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<List<TVShow>> GetByLibraryAsync(int libraryId)
    {
        return await _dbSet
            .Include(t => t.Seasons)
            .Include(t => t.Metadata)
            .Where(t => t.MediaLibraryId == libraryId && !t.IsDeleted)
            .OrderBy(t => t.Title)
            .ToListAsync();
    }

    public async Task<TVShow?> GetByTitleAsync(string title)
    {
        return await _dbSet
            .Include(t => t.Seasons)
            .Include(t => t.Metadata)
            .FirstOrDefaultAsync(t => t.Title == title && !t.IsDeleted);
    }

    public async Task<List<TVShow>> SearchAsync(string query)
    {
        var searchTerm = query.ToLower();
        return await _dbSet
            .Include(t => t.Seasons)
            .Where(t => !t.IsDeleted &&
                (t.Title.ToLower().Contains(searchTerm) ||
                 (t.OriginalTitle != null && t.OriginalTitle.ToLower().Contains(searchTerm))))
            .ToListAsync();
    }
}
