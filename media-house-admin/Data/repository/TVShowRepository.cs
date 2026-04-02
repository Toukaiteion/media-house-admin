using MediaHouse.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Data.repository;

public class TVShowRepository(MediaHouseDbContext context, ILogger<TVShowRepository> logger)
    : Repository<TVShow>(context, logger), Interfaces.ITVShowRepository
{
    public async Task<List<TVShow>> GetByLibraryAsync(int libraryId)
    {
        return await _dbSet
            .Include(t => t.Seasons)
            .Where(t => t.MediaLibraryId == libraryId && !t.IsDeleted)
            .OrderBy(t => t.Title)
            .ToListAsync();
    }

    public async Task<TVShow?> GetByTitleAsync(string title)
    {
        return await _dbSet
            .Include(t => t.Seasons)
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
