using MediaHouse.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Data;

public class MediaLibraryRepository : Repository<MediaLibrary>, Interfaces.IMediaLibraryRepository
{
    public MediaLibraryRepository(MediaHouseDbContext context, ILogger<MediaLibraryRepository> logger)
        : base(context, logger)
    {
    }

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
