using MediaHouse.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Data.repository;

public class MediaLibraryRepository(MediaHouseDbContext context, ILogger<MediaLibraryRepository> logger)
    : Repository<MediaLibrary>(context, logger), Interfaces.IMediaLibraryRepository
{
    public async Task<MediaLibrary?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Include(ml => ml.Movies)
            .Include(ml => ml.TVShows)
            .FirstOrDefaultAsync(ml => ml.Name == name && !ml.IsDeleted);
    }

    public async Task<List<MediaLibrary>> GetByTypeAsync(LibraryType type)
    {
        return await _dbSet
            .Where(ml => ml.Type == type && !ml.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<MediaLibrary>> GetEnabledAsync()
    {
        return await _dbSet
            .Where(ml => ml.IsEnabled && !ml.IsDeleted)
            .ToListAsync();
    }
}
