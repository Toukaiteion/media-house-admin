using MediaHouse.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Data.repository;

public class MovieRepository(MediaHouseDbContext context, ILogger<MovieRepository> logger)
    : Repository<Movie>(context, logger), Interfaces.IMovieRepository
{
    public async Task<List<Movie>> GetByLibraryAsync(int libraryId)
    {
        return await _dbSet
            .Include(m => m.MediaFile)
            .Include(m => m.Metadata)
            .Where(m => m.MediaLibraryId == libraryId && !m.IsDeleted)
            .OrderBy(m => m.Num)
            .ToListAsync();
    }

    public async Task<Movie?> GetByTitleAsync(string title)
    {
        return await _dbSet
            .Include(m => m.MediaFile)
            .Include(m => m.Metadata)
            .FirstOrDefaultAsync(m => m.Title == title && !m.IsDeleted);
    }

    public async Task<List<Movie>> SearchAsync(string query)
    {
        var searchTerm = query.ToLower();
        return await _dbSet
            .Include(m => m.MediaFile)
            .Where(m => !m.IsDeleted &&
                (m.Title.ToLower().Contains(searchTerm) ||
                 (m.OriginalTitle != null && m.OriginalTitle.ToLower().Contains(searchTerm))))
            .ToListAsync();
    }
}
