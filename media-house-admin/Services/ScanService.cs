using MediaHouse.Data.Entities;
using MediaHouse.Interfaces;
using MediaHouse.Data;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Services;

public class ScanService(IServiceScopeFactory scopeFactory, ILogger<ScanService> logger, IMetadataService metadataService, MediaHouseDbContext context) : IScanService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<ScanService> _logger = logger;
    private readonly IMetadataService _metadataService = metadataService;
    private readonly MediaHouseDbContext _context = context;

    private static readonly string[] VideoExtensions = [".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm"];

    public async Task<SystemSyncLog> StartFullScanAsync(int libraryId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaHouseDbContext>();

        var library = await context.MediaLibraries.FindAsync(libraryId) ??
            throw new InvalidOperationException($"Library {libraryId} not found");

        library.Status = ScanStatus.Scanning;
        await context.SaveChangesAsync();

        var log = new SystemSyncLog
        {
            MediaLibraryId = libraryId,
            SyncType = SyncType.FullScan,
            Status = SyncStatus.Started,
            StartTime = DateTime.UtcNow
        };

        context.SystemSyncLogs.Add(log);
        await context.SaveChangesAsync();

        _ = Task.Run(() => ExecuteFullScanAsync(libraryId, library.Path), CancellationToken.None);

        return log;
    }

    public async Task<SystemSyncLog> StartIncrementalScanAsync(int libraryId)
    {
        using var scope = _scopeFactory.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<MediaHouseDbContext>();

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

    public async Task<SystemSyncLog?> GetLatestScanLogAsync(int libraryId)
    {
        return await _context.SystemSyncLogs
            .Where(sl => sl.MediaLibraryId == libraryId)
            .OrderByDescending(sl => sl.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SystemSyncLog>> GetScanLogsAsync(int libraryId, int limit = 10)
    {
        return await _context.SystemSyncLogs
            .Where(sl => sl.MediaLibraryId == libraryId)
            .OrderByDescending(sl => sl.StartTime)
            .Take(limit)
            .ToListAsync();
    }

    private async Task ExecuteFullScanAsync(int libraryId, string libraryPath)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaHouseDbContext>();

        try
        {
            var log = await context.SystemSyncLogs
                .Where(sl => sl.MediaLibraryId == libraryId && sl.SyncType == SyncType.FullScan && sl.Status == SyncStatus.Started)
                .OrderByDescending(sl => sl.StartTime)
                .FirstOrDefaultAsync();

            if (log == null)
            {
                _logger.LogWarning("No pending scan log found for {LibraryId}", libraryId);
                return;
            }

            var library = await context.MediaLibraries.FindAsync(libraryId);
            if (library == null)
            {
                _logger.LogWarning("Library not found: {LibraryId}", libraryId);
                log.Status = SyncStatus.Failed;
                log.ErrorMessage = "Library not found";
                log.EndTime = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return;
            }

            log.Status = SyncStatus.InProgress;
            await context.SaveChangesAsync();

            _logger.LogInformation("Starting full scan for library {LibraryId} at {Path}", libraryId, libraryPath);

            var movieDirectories = GetMovieDirectories(libraryPath);
            _logger.LogInformation("Found {Count} movie directories) to scan", movieDirectories.Count);

            foreach (var movieDir in movieDirectories)
            {
                await ProcessMovieDirectoryAsync(context, libraryId, libraryPath, movieDir[0], movieDir[1], log);
                await context.SaveChangesAsync();
            }

            log.Status = SyncStatus.Completed;
            log.EndTime = DateTime.UtcNow;

            library.Status = ScanStatus.Idle;
            library.UpdateTime = DateTime.UtcNow;

            await context.SaveChangesAsync();
            _logger.LogInformation("Full scan completed for library {LibraryId}. Added: {Added}, Updated: {Updated}",
                libraryId, log.AddedCount, log.UpdatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full scan failed for library {LibraryId}", libraryId);

            var log = await context.SystemSyncLogs
                .Where(sl => sl.MediaLibraryId == libraryId && sl.SyncType == SyncType.FullScan && (sl.Status == SyncStatus.Started || sl.Status == SyncStatus.InProgress))
                .OrderByDescending(sl => sl.StartTime)
                .FirstOrDefaultAsync();

            if (log != null)
            {
                log.Status = SyncStatus.Failed;
                log.ErrorMessage = ex.Message;
                log.EndTime = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            var library = await context.MediaLibraries.FindAsync(libraryId);
            if (library != null)
            {
                library.Status = ScanStatus.Error;
                library.UpdateTime = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
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

        var files = Directory.GetFiles(directoryPath);
        var hasVideoFile = files.Any(f => VideoExtensions.Contains(Path.GetExtension(f).ToLower()));

        if (hasVideoFile)
            return true;

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

    private async Task ProcessMovieDirectoryAsync(MediaHouseDbContext context, int libraryId, string libraryPath, string movieDirName, string movieDirPath, SystemSyncLog log)
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

        var movieIdentifier = parseResult?.Num ?? movieDirName;

        await ProcessMediaItemAsync(context, libraryId, libraryPath, movieDirName, movieDirPath, movieIdentifier, parseResult, log);
    }

    private async Task ProcessMediaItemAsync(MediaHouseDbContext context, int libraryId, string libraryPath, string movieDirName, string movieDirPath, string movieIdentifier, NfoParseResult? parseResult, SystemSyncLog log)
    {
        // Create or update MediaItem (base media info)
        var existingMediaItem = await context.MediaItems
            .FirstOrDefaultAsync(mi => mi.LibraryId == libraryId && (mi.Name == movieIdentifier || mi.Title == movieIdentifier));

        MediaItem mediaItem;
        if (existingMediaItem == null)
        {
            _logger.LogInformation("Creating new media item: {Identifier}", movieIdentifier);

            mediaItem = new MediaItem
            {
                LibraryId = libraryId,
                Name = movieIdentifier,
                Title = parseResult?.Title ?? movieIdentifier,
                OriginalTitle = parseResult?.Title ?? movieIdentifier,
                Type = "movie",
                ReleaseDate = parseResult?.Premiered,
                Summary = parseResult?.Summary,
                PosterPath = parseResult?.ImagePaths?.ContainsKey("poster") == true ? Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["poster"]) : null,
                ThumbPath = parseResult?.ImagePaths?.ContainsKey("thumb") == true ? Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["thumb"]) : null,
                FanartPath = parseResult?.ImagePaths?.ContainsKey("fanart") == true ? Path.Combine(libraryPath, movieDirName, parseResult.ImagePaths["fanart"]) : null,
                CreateTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow
            };

            context.MediaItems.Add(mediaItem);
            log.AddedCount++;
        }
        else
        {
            _logger.LogInformation("Updating existing media item: {Identifier}", movieIdentifier);

            if (parseResult != null)
            {
                existingMediaItem.Title = parseResult.Title ?? existingMediaItem.Title;
                existingMediaItem.OriginalTitle = parseResult.Title ?? existingMediaItem.OriginalTitle;
                existingMediaItem.ReleaseDate = parseResult.Premiered ?? existingMediaItem.ReleaseDate;
                existingMediaItem.Summary = parseResult.Summary ?? existingMediaItem.Summary;

                var PosterPath = parseResult.ImagePaths?.ContainsKey("poster") == true ? parseResult.ImagePaths["poster"] : "";
                if (!string.IsNullOrEmpty(PosterPath))
                {
                    existingMediaItem.PosterPath = Path.Combine(libraryPath, movieDirName,  PosterPath);
                }

                var ThumbPath = parseResult.ImagePaths?.ContainsKey("thumb") == true ? parseResult.ImagePaths["thumb"] : "";
                if (!string.IsNullOrEmpty(ThumbPath))
                {
                    existingMediaItem.ThumbPath = Path.Combine(libraryPath, movieDirName, ThumbPath);
                }

                var FanartPath = parseResult.ImagePaths?.ContainsKey("fanart") == true ? parseResult.ImagePaths["fanart"] : "";
                if (!string.IsNullOrEmpty(FanartPath))
                {
                    existingMediaItem.FanartPath = Path.Combine(libraryPath, movieDirName, FanartPath);
                }

                existingMediaItem.UpdateTime = DateTime.UtcNow;
                mediaItem = existingMediaItem;
            }

            context.MediaItems.Update(existingMediaItem);
            log.UpdatedCount++;
        }

        await context.SaveChangesAsync();

        existingMediaItem = await context.MediaItems
            .FirstOrDefaultAsync(mi => mi.LibraryId == libraryId && (mi.Name == movieIdentifier || mi.Title == movieIdentifier));

        // Create or update Movie (detailed info for movies)
        await ProcessMovieAsync(context, libraryId, movieIdentifier, parseResult, log, existingMediaItem.Id);
    }

    private async Task ProcessMovieAsync(MediaHouseDbContext context, int libraryId, string movieIdentifier, NfoParseResult? parseResult, SystemSyncLog log, int mediaItemId)
    {
        var existingMovie = await context.Movies
            .Include(m => m.MediaFile)
            .Include(m => m.MediaItem)
            .FirstOrDefaultAsync(m => m.Num == movieIdentifier);

        if (existingMovie != null)
        {
            _logger.LogInformation("Creating new movie: {Identifier}", movieIdentifier);

            var movie = new Movie
            {
                MediaLibraryId = libraryId,
                Num = parseResult?.Num,
                Title = parseResult?.Title ?? movieIdentifier,
                Studio = parseResult?.Studios,
                Maker = parseResult?.Maker,
                Runtime = parseResult?.Runtime,
                Overview = parseResult?.Summary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(parseResult?.Premiered))
            {
                movie.ReleaseDate = parseResult.Premiered;
            }

            context.Movies.Add(movie);
            log.AddedCount++;
        }
        else
        {
            _logger.LogInformation("Updating existing movie: {Identifier}", movieIdentifier);

            if (parseResult != null)
            {
                existingMovie.Title = parseResult.Title ?? existingMovie.Title;
                existingMovie.Studio = parseResult?.Studios ?? existingMovie.Studio;
                existingMovie.Maker = parseResult?.Maker ?? existingMovie.Maker;
                existingMovie.Runtime = parseResult.Runtime ?? existingMovie.Runtime;
                existingMovie.Overview = parseResult.Summary ?? existingMovie.Overview;

                if (!string.IsNullOrEmpty(parseResult?.Premiered))
                {
                    existingMovie.ReleaseDate = parseResult.Premiered;
                }

                existingMovie.UpdatedAt = DateTime.UtcNow;
            }

            context.Movies.Update(existingMovie);
            log.UpdatedCount++;
        }

        await context.SaveChangesAsync();

        // Link movie to media item
        await LinkMovieToMediaItemAsync(context, movieIdentifier, mediaItemId);

        // Create MediaFile
        await CreateMovieMediaFileAsync(context, libraryId, movieIdentifier, parseResult, log);

        // Create tags
        if (parseResult?.Tags != null)
        {
            await CreateMovieTagsAsync(context, libraryId, movieIdentifier, parseResult.Tags);
        }

        // Create actors and MediaStaff
        if (parseResult?.Actors != null && parseResult.Actors.Count > 0)
        {
            await CreateMovieActorsAsync(context, libraryId, movieIdentifier, parseResult.Actors);
        }
    }

    private async Task LinkMovieToMediaItemAsync(MediaHouseDbContext context, string movieIdentifier, int mediaItemId)
    {
        var mediaItem = await context.MediaItems.FindAsync(mediaItemId);
        var movie = await context.Movies.FirstOrDefaultAsync(m => m.Num == movieIdentifier);

        if (mediaItem != null && movie != null)
        {
            var existingMediaItemFromDb = await context.MediaItems
                .IncludeAsSplit()
                    .ThenInclude(mi => mi.Movies)
                    .FirstOrDefaultAsync(mi => mi.Id == mediaItemId);

            if (existingMediaItemFromDb != null)
            {
                existingMediaItemFromDb.Movies.Add(movie);
            }
        }
    }

    private async Task CreateMovieMediaFileAsync(MediaHouseDbContext context, int libraryId, string movieIdentifier, NfoParseResult? parseResult, SystemSyncLog log)
    {
        var movie = await context.Movies.FirstOrDefaultAsync(m => m.Num == movieIdentifier);
        if (movie == null)
        {
            _logger.LogWarning("Movie not found after media item creation: {Identifier}", movieIdentifier);
            return;
        }

        var videoFile = parseResult?.VideoFile ?? ""; // TODO: Get actual video file path
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

        context.MediaFiles.Add(mediaFile);
    }

    private async Task CreateMovieTagsAsync(MediaHouseDbContext context, int libraryId, string movieIdentifier, string tags)
    {
        var movie = await context.Movies.FirstOrDefaultAsync(m => m.Num == movieIdentifier);
        if (movie == null)
        {
            _logger.LogWarning("Movie not found when creating tags: {Identifier}", movieIdentifier);
            return;
        }

        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        foreach (var tagName in tagList)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                continue;

            var trimmedTagName = tagName.Trim();

            var existingTag = await context.MediaTags
                .FirstOrDefaultAsync(t => t.LibraryId == libraryId && t.TagName == trimmedTagName);

            if (existingTag == null)
            {
                var mediaTag = new MediaTag
                {
                    LibraryId = libraryId,
                    MediaType = MediaType.Movie,
                    MediaId = movie.Id,
                    TagName = trimmedTagName,
                    CreatedAt = DateTime.UtcNow
                };
                context.MediaTags.Add(mediaTag);
            }
        }
    }

    private async Task CreateMovieActorsAsync(MediaHouseDbContext context, int libraryId, string movieIdentifier, List<string> actors)
    {
        var movie = await context.Movies.FirstOrDefaultAsync(m => m.Num == movieIdentifier);
        if (movie == null)
        {
            _logger.LogWarning("Movie not found when creating actors: {Identifier}", movieIdentifier);
            return;
        }

        foreach (var actorName in actors)
        {
            var staff = await GetOrCreateStaffAsync(context, actorName);

            var mediaStaff = new MediaStaff
            {
                MediaType = MediaStaffType.Movie,
                MediaId = movie.Id,
                StaffId = staff.Id,
                RoleType = RoleType.Actor,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.MediaStaffs.Add(mediaStaff);
        }
    }

    private async Task<Staff> GetOrCreateStaffAsync(MediaHouseDbContext context, string actorName)
    {
        var staff = await context.Staffs.FirstOrDefaultAsync(s => s.Name == actorName);

        if (staff == null)
        {
            staff = new Staff
            {
                Name = actorName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Staffs.Add(staff);
        }

        return staff;
    }
}
