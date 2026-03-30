using MediaHouse.Data.Entities;

namespace MediaHouse.Interfaces;

public interface ILibraryService
{
    Task<List<MediaLibrary>> GetAllLibrariesAsync();
    Task<MediaLibrary?> GetLibraryByIdAsync(string id);
    Task<MediaLibrary> CreateLibraryAsync(string name, LibraryType type, string path);
    Task<MediaLibrary?> UpdateLibraryAsync(string id, string name, string path, bool isEnabled);
    Task<bool> DeleteLibraryAsync(string id);
    Task<bool> TriggerScanAsync(string id);
}
