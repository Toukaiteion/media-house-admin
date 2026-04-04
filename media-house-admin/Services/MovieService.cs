using MediaHouse.Data;
using MediaHouse.Data.Entities;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Services;

public class MovieService(MediaHouseDbContext context, ILogger<MovieService> logger) : IMovieService
{
    private readonly MediaHouseDbContext _context = context;
    private readonly ILogger<MovieService> _logger = logger;

    public async Task<(List<MovieDto> Movies, int TotalCount)> GetMoviesAsync(MovieQueryDto query)
    {
        // Start with base query
        var mediaQuery = _context.Medias
            .Include(m => m.Movie)
            .Where(m => m.Type == "movie");

        // Apply filters based on filter type (mutually exclusive)
        switch (query.Filter?.ToLower())
        {
            case "tags":
                if (!string.IsNullOrEmpty(query.Tags))
                {
                    var tagNames = query.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .ToList();
                    mediaQuery = mediaQuery
                        .Include(m => m.MediaTags)
                            .ThenInclude(mt => mt.Tag)
                        .Where(m => m.MediaTags
                            .Any(mt => mt.Tag != null && tagNames.Contains(mt.Tag.TagName, StringComparer.OrdinalIgnoreCase)));
                }
                break;

            case "actor":
                if (query.ActorId.HasValue)
                {
                    mediaQuery = mediaQuery
                        .Include(m => m.MediaStaffs)
                        .Where(m => m.MediaStaffs
                            .Any(ms => ms.StaffId == query.ActorId.Value &&
                                   ms.RoleType.Equals("actor", StringComparison.OrdinalIgnoreCase)));
                }
                break;

            case "recent":
                if (query.UserId.HasValue)
                {
                    var mediaIds = await _context.PlayRecords
                        .Where(pr => pr.UserId == query.UserId.Value)
                        .OrderByDescending(pr => pr.LastPlayTime)
                        .Select(pr => pr.MediaId)
                        .ToListAsync();

                    mediaQuery = mediaQuery
                        .Where(m => mediaIds.Contains(m.Id))
                        .OrderBy(m => mediaIds.IndexOf(m.Id));
                }
                break;

            case "mostly_play":
                // Requires play_count field on Media table
                mediaQuery = mediaQuery.OrderByDescending(m => m.PlayCount ?? 0);
                break;

            case "favor":
                if (query.UserId.HasValue)
                {
                    var favorMediaIds = await _context.MyFavors
                        .Where(f => f.UserId == query.UserId.Value)
                        .OrderByDescending(f => f.CreateTime)
                        .Select(f => f.MediaId)
                        .ToListAsync();

                    mediaQuery = mediaQuery
                        .Where(m => favorMediaIds.Contains(m.Id))
                        .OrderBy(m => favorMediaIds.IndexOf(m.Id));
                }
                break;

            default:
                // Default: filter by library_id
                if (query.LibraryId.HasValue)
                {
                    mediaQuery = mediaQuery.Where(m => m.LibraryId == query.LibraryId.Value);
                }
                mediaQuery = mediaQuery.OrderBy(m => m.Id);
                break;
        }

        var totalCount = await mediaQuery.CountAsync();

        var medias = await mediaQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = medias.Select(MapToDto).ToList();
        return (dtos, totalCount);
    }

    public async Task<bool> DeleteMovieAsync(int mediaId)
    {
        var media = await _context.Medias
            .Include(m => m.MediaFiles)
            .Include(m => m.MediaImgs)
            .Include(m => m.Movie)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
        {
            return false;
        }

        // Collect file paths to delete
        var filePathsToDelete = new List<string>();

        if (media.MediaFiles != null)
        {
            foreach (var mediaFile in media.MediaFiles)
            {
                if (!string.IsNullOrEmpty(mediaFile.Path) && System.IO.File.Exists(mediaFile.Path))
                {
                    filePathsToDelete.Add(mediaFile.Path);
                }
            }
        }

        if (media.MediaImgs != null)
        {
            foreach (var mediaImg in media.MediaImgs)
            {
                if (!string.IsNullOrEmpty(mediaImg.Path) && System.IO.File.Exists(mediaImg.Path))
                {
                    filePathsToDelete.Add(mediaImg.Path);
                }
            }
        }

        // Get movie directory path (from MediaFile)
        string? directoryToDelete = null;
        var firstMediaFile = media.MediaFiles?.FirstOrDefault();
        if (firstMediaFile != null && !string.IsNullOrEmpty(firstMediaFile.Path))
        {
            directoryToDelete = System.IO.Path.GetDirectoryName(firstMediaFile.Path);
        }

        // Remove media from database (cascade delete will handle related records)
        _context.Medias.Remove(media);
        await _context.SaveChangesAsync();

        // Delete files after successful database deletion
        foreach (var filePath in filePathsToDelete)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation("Deleted file: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {FilePath}", filePath);
            }
        }

        // Delete directory if it exists and is empty
        if (!string.IsNullOrEmpty(directoryToDelete) && System.IO.Directory.Exists(directoryToDelete))
        {
            try
            {
                var filesInDir = System.IO.Directory.GetFiles(directoryToDelete);
                if (filesInDir.Length == 0)
                {
                    System.IO.Directory.Delete(directoryToDelete);
                    _logger.LogInformation("Deleted empty directory: {Directory}", directoryToDelete);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete directory {Directory}", directoryToDelete);
            }
        }

        return true;
    }

    private static MovieDto MapToDto(Media media)
    {
        int? year = null;
        if (!string.IsNullOrEmpty(media.ReleaseDate) && DateTime.TryParse(media.ReleaseDate, out var releaseDate))
        {
            year = releaseDate.Year;
        }

        return new MovieDto
        {
            Id = media.Id.ToString(),
            Title = media.Title,
            Year = year,
            PosterPath = media.PosterPath,
            ThumbPath = media.ThumbPath,
            FanartPath = media.FanartPath,
            Overview = media.Summary ?? media.Movie?.Description,
            CreatedAt = media.CreateTime,
            MediaLibraryId = media.LibraryId.ToString(),
            PlayCount = media.PlayCount
        };
    }
}
