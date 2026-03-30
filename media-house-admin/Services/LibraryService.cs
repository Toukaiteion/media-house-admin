using MediaHouse.Data.Entities;
using MediaHouse.Interfaces;
using MediaHouse.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Services;

public class LibraryService : ILibraryService
{
    private readonly MediaHouseDbContext _context;
    private readonly ILogger<LibraryService> _logger;

    public LibraryService(MediaHouseDbContext context, ILogger<LibraryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MediaLibrary>> GetAllLibrariesAsync()
    {
        return await _context.MediaLibraries
            .Where(l => !l.IsDeleted) // Add IsDeleted property to entity if needed
            .ToListAsync();
    }

    public async Task<MediaLibrary?> GetLibraryByIdAsync(string id)
    {
        return await _context.MediaLibraries
            .Include(l => l.Movies)
            .Include(l => l.TVShows)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<MediaLibrary> CreateLibraryAsync(string name, LibraryType type, string path)
    {
        var library = new MediaLibrary
        {
            Name = name,
            Type = type,
            Path = path,
            Status = ScanStatus.Idle,
            IsEnabled = true
        };

        _context.MediaLibraries.Add(library);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created library: {Name} at {Path}", name, path);

        return library;
    }

    public async Task<MediaLibrary?> UpdateLibraryAsync(string id, string name, string path, bool isEnabled)
    {
        var library = await _context.MediaLibraries.FindAsync(id);
        if (library == null) return null;

        library.Name = name;
        library.Path = path;
        library.IsEnabled = isEnabled;
        library.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return library;
    }

    public async Task<bool> DeleteLibraryAsync(string id)
    {
        var library = await _context.MediaLibraries.FindAsync(id);
        if (library == null) return false;

        _context.MediaLibraries.Remove(library);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> TriggerScanAsync(string id)
    {
        var library = await _context.MediaLibraries.FindAsync(id);
        if (library == null) return false;

        library.Status = ScanStatus.Scanning;
        library.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // TODO: Implement actual scanning logic
        _logger.LogInformation("Triggered scan for library {LibraryId}", id);

        return true;
    }
}
