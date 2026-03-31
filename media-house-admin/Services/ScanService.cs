using MediaHouse.Data.Entities;
using MediaHouse.Interfaces;
using MediaHouse.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Services;

public class ScanService(MediaHouseDbContext context, ILogger<ScanService> logger, IMetadataService metadataService) : IScanService
{
    private readonly MediaHouseDbContext _context = context;
    private readonly ILogger<ScanService> _logger = logger;
    private readonly IMetadataService _metadataService = metadataService;

    private static readonly string[] VideoExtensions = [".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm"];

    public async Task<SystemSyncLog> StartFullScanAsync(string libraryId)
    {
        var library = await _context.MediaLibraries.FindAsync(libraryId) ??
            throw new InvalidOperationException($"Library {libraryId} not found");

        // Update library status
        library.Status = ScanStatus.Scanning;
        await _context.SaveChangesAsync();

        var log = new SystemSyncLog
        {
            MediaLibraryId = libraryId,
            SyncType = SyncType.FullScan,
            Status = SyncStatus.Started,
            StartTime = DateTime.UtcNow
        };

        _context.SystemSyncLogs.Add(log);
        await _context.SaveChangesAsync();

        try
        {
            log.Status = SyncStatus.InProgress;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Starting full scan for library {LibraryId} at {Path}", libraryId, library.Path);

            // Get all movie directories
            var movieDirectories = GetMovieDirectories(library.Path);
            _logger.LogInformation("Found {Count} movie directories to scan", movieDirectories.Count);

            foreach (var movieDir in movieDirectories)
            {
                await ProcessMovieDirectoryAsync(libraryId, library.Path, movieDir[0], movieDir[1], log);
                await _context.SaveChangesAsync();
            }

            log.Status = SyncStatus.Completed;
            log.EndTime = DateTime.UtcNow;

            library.Status = ScanStatus.Idle;
            library.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Full scan completed for library {LibraryId}. Added: {Added}, Updated: {Updated}",
                libraryId, log.AddedCount, log.UpdatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full scan failed for library {LibraryId}", libraryId);
            log.Status = SyncStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.EndTime = DateTime.UtcNow;

            library.Status = ScanStatus.Error;
            library.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            throw;
        }

        return log;
    }

    public async Task<SystemSyncLog> StartIncrementalScanAsync(string libraryId)
    {
        var library = await _context.MediaLibraries.FindAsync(libraryId);
        if (library == null)
            throw new InvalidOperationException($"Library {libraryId} not found");

        var log = new SystemSyncLog
        {
            MediaLibraryId = libraryId,
            SyncType = SyncType.IncrementalScan,
            Status = SyncStatus.Started,
            StartTime = DateTime.UtcNow
        };

        _context.SystemSyncLogs.Add(log);
        await _context.SaveChangesAsync();

        try
        {
            log.Status = SyncStatus.InProgress;
            await _context.SaveChangesAsync();

            // TODO: Implement incremental scan logic
            _logger.LogInformation("Starting incremental scan for library {LibraryId}", libraryId);

            log.Status = SyncStatus.Completed;
            log.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Incremental scan failed for library {LibraryId}", libraryId);
            log.Status = SyncStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            throw;
        }

        return log;
    }

    public async Task<SystemSyncLog?> GetLatestScanLogAsync(string libraryId)
    {
        return await _context.SystemSyncLogs
            .Where(sl => sl.MediaLibraryId == libraryId)
            .OrderByDescending(sl => sl.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SystemSyncLog>> GetScanLogsAsync(string libraryId, int limit = 10)
    {
        return await _context.SystemSyncLogs
            .Where(sl => sl.MediaLibraryId == libraryId)
            .OrderByDescending(sl => sl.StartTime)
            .Take(limit)
            .ToListAsync();
    }

    private List<string[]> GetMovieDirectories(string libraryPath)
    {
        var movieDirs = new List<string[]>();

        if (!Directory.Exists(libraryPath))
        {
            _logger.LogWarning("Library path does not exist: {Path}", libraryPath);
            return movieDirs;
        }

        foreach (var dir in Directory.GetDirectories(libraryPath))
        {
            if (IsMovieDirectory(dir))
            {
                var dirName = Path.GetFileName(dir);
                movieDirs.Add([dirName, dir]);
            }
        }

        return movieDirs;
    }

    private bool IsMovieDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return false;

        // Check for video files
        var files = Directory.GetFiles(directoryPath);
        var hasVideoFile = files.Any(f => VideoExtensions.Contains(Path.GetExtension(f).ToLower()));

        if (hasVideoFile)
            return true;

        // Check for NFO file
        var hasNfoFile = files.Any(f => f.EndsWith(".nfo", StringComparison.OrdinalIgnoreCase));

        return hasNfoFile;
    }

    private string? FindVideoFile(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return null;

        var files = Directory.GetFiles(directoryPath);
        return files.FirstOrDefault(f => VideoExtensions.Contains(Path.GetExtension(f).ToLower()));
    }

    private string? FindNfoFile(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return null;

        var files = Directory.GetFiles(directoryPath);
        return files.FirstOrDefault(f => f.EndsWith(".nfo", StringComparison.OrdinalIgnoreCase));
    }

    private async Task ProcessMovieDirectoryAsync(string libraryId, string libraryPath, string movieDirName, string movieDirPath, SystemSyncLog log)
    {
        _logger.LogInformation("Processing movie directory: {DirName}", movieDirName);

        var videoFile = FindVideoFile(movieDirPath);
        var nfoFile = FindNfoFile(movieDirPath);

        if (videoFile == null)
        {
            _logger.LogWarning("No video file found in {DirPath}", movieDirPath);
            return;
        }

        var parseResult = nfoFile != null ? await _metadataService.ParseNfoFileFullAsync(nfoFile) : null;

        // Determine movie identifier: prefer num field from NFO, fallback to folder name
        var movieIdentifier = parseResult?.Num ?? movieDirName;

        // Check if movie already exists
        var existingMovie = await _context.Movies
            .Include(m => m.Metadata)
            .Include(m => m.MediaFile)
            .Include(m => m.MediaLibrary)
            .FirstOrDefaultAsync(m => m.MediaLibraryId == libraryId && (m.Title == movieIdentifier || m.Title == movieDirName || m.Num == movieIdentifier));

        if (existingMovie != null)
        {
            // Update existing movie
            _logger.LogInformation("Updating existing movie: {Title}", existingMovie.Title);

            if (parseResult != null)
            {
                existingMovie.Title = parseResult.Metadata.Title ?? existingMovie.Title;
                existingMovie.Runtime = parseResult.Runtime ?? existingMovie.Runtime;
                existingMovie.Overview = parseResult.Metadata.Plot ?? existingMovie.Overview;

                if (parseResult.Metadata.Premiered.HasValue)
                {
                    existingMovie.ReleaseDate = parseResult.Metadata.Premiered.Value;
                }

                if (parseResult.Metadata.Studios != null)
                {
                    existingMovie.Studio = parseResult.Metadata.Studios;
                }

                if (!string.IsNullOrEmpty(parseResult.Maker))
                {
                    existingMovie.Maker = parseResult.Maker;
                }

                // Update image paths
                if (parseResult.ImagePaths.ContainsKey("poster") && !string.IsNullOrEmpty(parseResult.ImagePaths["poster"]))
                {
                    existingMovie.PosterPath = Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["poster"]);
                }

                if (parseResult.ImagePaths.ContainsKey("thumb") && !string.IsNullOrEmpty(parseResult.ImagePaths["thumb"]))
                {
                    existingMovie.ThumbPath = Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["thumb"]);
                }

                if (parseResult.ImagePaths.ContainsKey("fanart") && !string.IsNullOrEmpty(parseResult.ImagePaths["fanart"]))
                {
                    existingMovie.FanartPath = Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["fanart"]);
                }

                // Check for extrafanart folder
                var extrafanartPath = Path.Combine(libraryPath, movieDirName, "extrafanart");
                if (Directory.Exists(extrafanartPath))
                {
                    existingMovie.ScreenshotsPath = extrafanartPath;
                }
            }

            existingMovie.UpdatedAt = DateTime.UtcNow;
            log.UpdatedCount++;

            // Update metadata
            if (existingMovie.Metadata != null && parseResult != null)
            {
                existingMovie.Metadata.Genre = parseResult.Metadata.Genre ?? existingMovie.Metadata.Genre;
                existingMovie.Metadata.Tags = parseResult.Metadata.Tags ?? existingMovie.Metadata.Tags;
            }
            _context.Movies.Update(existingMovie);
        }
        else
        {
            // Create new movie
            _logger.LogInformation("Creating new movie: {Identifier}", movieIdentifier);

            var movie = new Movie
            {
                MediaLibraryId = libraryId,
                Title = parseResult?.Metadata.Title ?? movieIdentifier,
                Num = parseResult?.Num,
                Runtime = parseResult?.Runtime,
                Overview = parseResult?.Metadata.Plot,
                Studio = parseResult?.Metadata.Studios,
                Maker = parseResult?.Maker,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (parseResult?.Metadata.Premiered.HasValue == true)
            {
                movie.ReleaseDate = parseResult.Metadata.Premiered.Value;
            }

            // Set image paths
            if (parseResult != null)
            {
                if (!string.IsNullOrEmpty(parseResult.ImagePaths["poster"]))
                {
                    movie.PosterPath = Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["poster"]);
                }

                if (!string.IsNullOrEmpty(parseResult.ImagePaths["thumb"]))
                {
                    movie.ThumbPath = Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["thumb"]);
                }

                if (!string.IsNullOrEmpty(parseResult.ImagePaths["fanart"]))
                {
                    movie.FanartPath = Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["fanart"]);
                }

                // Check for extrafanart folder
                var extrafanartPath = Path.Combine(libraryPath, movieDirName, "extrafanart");
                if (Directory.Exists(extrafanartPath))
                {
                    movie.ScreenshotsPath = extrafanartPath;
                }
            }

            // Create NfoMetadata
            if (parseResult != null)
            {
                var metadata = parseResult.Metadata;
                metadata.MovieId = movie.Id;
                movie.Metadata = metadata;
                _context.NfoMetadata.Add(metadata);
            }

            // Create MediaFile
            var fileInfo = new FileInfo(videoFile);
            var mediaFile = new MediaFile
            {
                MediaType = MediaType.Movie,
                MediaId = movie.Id,
                MovieId = movie.Id,
                Path = videoFile,
                FileName = fileInfo.Name,
                Extension = fileInfo.Extension,
                SizeBytes = fileInfo.Length,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            movie.MediaFile = mediaFile;

            _context.Movies.Add(movie);
            log.AddedCount++;

            // Create tags
            if (parseResult?.Metadata?.Tags != null)
            {
                var tagList = parseResult.Metadata?.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                await CreateTagsAsync(libraryId, movie.Id, tagList);
            }

            if (parseResult?.Actors != null && parseResult.Actors.Count > 0)
            {
                foreach (var actorName in parseResult.Actors)
                {
                    var staff = await GetOrCreateStaffAsync(actorName);

                    var mediaStaff = new MediaStaff
                    {
                        MediaType = MediaStaffType.Movie,
                        MediaId = movie.Id,
                        StaffId = staff.Id,
                        RoleType = RoleType.Actor,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.MediaStaffs.Add(mediaStaff);
                }
            }
        }
    }

    private async Task<Staff> GetOrCreateStaffAsync(string actorName)
    {
        var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Name == actorName);

        if (staff == null)
        {
            staff = new Staff
            {
                Name = actorName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Staffs.Add(staff);
        }

        return staff;
    }

    private async Task CreateTagsAsync(string libraryId, string movieId, List<string> tagNames)
    {
        foreach (var tagName in tagNames)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                continue;

            var trimmedTagName = tagName.Trim();

            var existingTag = await _context.MediaTags
                .FirstOrDefaultAsync(t => t.MediaLibraryId == libraryId && t.TagName == trimmedTagName);

            if (existingTag == null)
            {
                var mediaTag = new MediaTag
                {
                    MediaLibraryId = libraryId,
                    MediaType = MediaType.Movie,
                    MediaId = movieId,
                    TagName = trimmedTagName,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MediaTags.Add(mediaTag);
            }
        }
    }

}
