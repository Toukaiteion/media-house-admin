using Microsoft.AspNetCore.Mvc;
using MediaHouse.Data;
using MediaHouse.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController(
    MediaHouseDbContext dbContext,
    ILogger<MoviesController> logger) : ControllerBase
{
    private readonly MediaHouseDbContext _dbContext = dbContext;
    private readonly ILogger<MoviesController> _logger = logger;

    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> GetMovies(
        [FromQuery] string? libraryId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _dbContext.Medias
                .Include(m => m.Movie)
                .Where(m => m.Type == "movie");

            if (!string.IsNullOrEmpty(libraryId) && int.TryParse(libraryId, out var libId))
            {
                query = query.Where(m => m.LibraryId == libId);
            }

            var medias = await query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = medias.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching movies");
            return StatusCode(500, new { error = "Failed to fetch movies" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieDetailDto>> GetMovie(int id)
    {
        try
        {
            var media = await _dbContext.Medias
                .Include(m => m.Movie)
                .Include(m => m.MediaFiles)
                .Include(m => m.MediaImgs)
                .Include(m => m.MediaTags)
                    .ThenInclude(mt => mt.Tag)
                .Include(m => m.MediaStaffs)
                    .ThenInclude(ms => ms.Staff)
                .FirstOrDefaultAsync(m => m.Id == id && m.Type == "movie");

            if (media == null)
            {
                return NotFound(new { error = "Movie not found" });
            }

            return Ok(MapToDetailDto(media));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching movie {MovieId}", id);
            return StatusCode(500, new { error = "Failed to fetch movie" });
        }
    }

    private static MovieDto MapToDto(Data.Entities.Media media)
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
            MediaLibraryId = media.LibraryId.ToString()
        };
    }

    private static MovieDetailDto MapToDetailDto(Data.Entities.Media media)
    {
        int? year = null;
        if (!string.IsNullOrEmpty(media.ReleaseDate) && DateTime.TryParse(media.ReleaseDate, out var releaseDate))
        {
            year = releaseDate.Year;
        }

        var mediaFile = media.MediaFiles?.FirstOrDefault();

        // Get screenshots from Movie.ScreenshotsPath (comma-separated url_names)
        var screenshotUrlNames = media.Movie?.ScreenshotsPath?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        // Combine with screenshots from MediaImgs where Type == "screenshot"
        var screenshots = media.MediaImgs?
            .Where(mi => mi.Type?.ToLower() == "screenshot" || screenshotUrlNames.Contains(mi.UrlName))
            .OrderBy(mi => mi.Name) // Sort by name for consistent ordering
            .Select(mi => new ScreenshotDto
            {
                UrlName = mi.UrlName,
                Name = mi.Name,
                Path = mi.Path,
                Width = mi.Width,
                Height = mi.Height,
                SizeBytes = mi.SizeBytes
            })
            .ToList() ?? [];

        // Group staff by role type
        var actors = new List<StaffDto>();
        var directors = new List<StaffDto>();
        var writers = new List<StaffDto>();

        if (media.MediaStaffs != null)
        {
            foreach (var mediaStaff in media.MediaStaffs.OrderByDescending(ms => ms.SortOrder))
            {
                if (mediaStaff.Staff == null) continue;

                var staffDto = new StaffDto
                {
                    Id = mediaStaff.Staff.Id.ToString(),
                    Name = mediaStaff.Staff.Name,
                    AvatarPath = mediaStaff.Staff.AvatarPath,
                    RoleName = mediaStaff.RoleName
                };

                var roleType = mediaStaff.RoleType.ToLowerInvariant();
                if (roleType == "actor")
                {
                    actors.Add(staffDto);
                }
                else if (roleType == "director")
                {
                    directors.Add(staffDto);
                }
                else if (roleType == "writer")
                {
                    writers.Add(staffDto);
                }
            }
        }

        // Get tags
        var tags = media.MediaTags?
            .Where(mt => mt.Tag != null)
            .Select(mt => new TagDto
            {
                Id = mt.Tag.Id.ToString(),
                TagName = mt.Tag.TagName
            })
            .ToList() ?? [];

        return new MovieDetailDto
        {
            Id = media.Id.ToString(),
            Title = media.Title,
            OriginalTitle = media.OriginalTitle,
            Year = year,
            ReleaseDate = media.ReleaseDate,
            PosterPath = media.PosterPath,
            ThumbPath = media.ThumbPath,
            FanartPath = media.FanartPath,
            Overview = media.Summary ?? media.Movie?.Description,
            CreatedAt = media.CreateTime,
            MediaLibraryId = media.LibraryId.ToString(),
            FilePath = mediaFile?.Path,
            ContainerFormat = mediaFile?.Container,
            Duration = media.Movie?.Runtime ?? mediaFile?.Runtime, // Prefer Movie.Runtime, fallback to MediaFile.Runtime
            FileSize = mediaFile?.SizeBytes,
            Num = media.Movie?.Num,
            Studio = media.Movie?.Studio,
            Maker = media.Movie?.Maker,
            Screenshots = screenshots,
            Actors = actors,
            Directors = directors,
            Writers = writers,
            Tags = tags
        };
    }
}
