using Microsoft.AspNetCore.Mvc;
using MediaHouse.Data;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController(
    MediaHouseDbContext dbContext,
    IMediaService mediaService,
    IFavorService favorService,
    IPlayRecordService playRecordService,
    ILogger<MediaController> logger) : ControllerBase
{
    private readonly MediaHouseDbContext _dbContext = dbContext;
    private readonly IMediaService _mediaService = mediaService;
    private readonly IFavorService _favorService = favorService;
    private readonly IPlayRecordService _playRecordService = playRecordService;
    private readonly ILogger<MediaController> _logger = logger;

    [HttpGet("file")]
    public IActionResult GetMediaFile([FromQuery] string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return NotFound(new { error = "File not found" });
            }

            var fileInfo = new System.IO.FileInfo(path);
            var contentType = GetContentType(fileInfo.Extension);

            var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            return File(fileStream, contentType, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving media file: {Path}", path);
            return StatusCode(500, new { error = "Failed to serve media file" });
        }
    }

    [HttpPut("{id}/metadata")]
    public async Task<ActionResult> UpdateMetadata(int id, [FromBody] UpdateMediaMetadataDto dto)
    {
        // Parameter validation
        if (id <= 0)
        {
            return BadRequest(new { error = "Invalid media ID" });
        }

        if (dto == null)
        {
            return BadRequest(new { error = "Invalid request body" });
        }

        // Call service layer for business logic
        var success = await _mediaService.UpdateMediaMetadataAsync(id, dto);

        if (!success)
        {
            return NotFound(new { error = "Media not found" });
        }

        return Ok(new { message = "Metadata updated successfully" });
    }

    [HttpGet("image/{url_name}")]
    public async Task<IActionResult> GetImageByUrlName(string url_name)
    {
        try
        {
            // Parameter validation
            if (string.IsNullOrEmpty(url_name))
            {
                return BadRequest(new { error = "Invalid url name" });
            }

            // Find MediaImg by url_name from database
            var mediaImg = await _dbContext.MediaImgs
                .FirstOrDefaultAsync(mi => mi.UrlName == url_name);

            if (mediaImg == null)
            {
                return NotFound(new { error = "Image not found" });
            }

            if (!System.IO.File.Exists(mediaImg.Path))
            {
                return NotFound(new { error = "Image file not found" });
            }

            var contentType = GetContentType(mediaImg.Extension ?? "");
            var fileStream = new System.IO.FileStream(mediaImg.Path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            return File(fileStream, contentType, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving image: {UrlName}", url_name);
            return StatusCode(500, new { error = "Failed to serve image" });
        }
    }

    [HttpPost("{mediaId}/play")]
    public async Task<IActionResult> Play(int mediaId, [FromBody] PlayRequestDto dto)
    {
        try
        {
            // Parameter validation
            if (mediaId <= 0)
            {
                return BadRequest(new { error = "Invalid media ID" });
            }

            if (dto == null)
            {
                return BadRequest(new { error = "Invalid request body" });
            }

            // Get media info with file path
            var media = await _dbContext.Medias
                .Include(m => m.MediaFiles)
                .FirstOrDefaultAsync(m => m.Id == mediaId);

            if (media == null)
            {
                return NotFound(new { error = "Media not found" });
            }

            // Get file path from MediaFiles
            string? filePath = null;
            if (media.MediaFiles.Any())
            {
                filePath = media.MediaFiles.First().Path;
            }

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return NotFound(new { error = "Media file not found" });
            }

            // Create/update play record
            var positionSeconds = dto.PositionSeconds ?? 0;
            await _playRecordService.CreateOrUpdatePlayRecordAsync(mediaId, dto.UserId, positionSeconds);

            // Get content type and return file stream
            var fileInfo = new System.IO.FileInfo(filePath);
            var contentType = GetContentType(fileInfo.Extension);
            var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            return File(fileStream, contentType, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing media {MediaId}", mediaId);
            return StatusCode(500, new { error = "Failed to play media" });
        }
    }

    [HttpPost("{mediaId}/favor")]
    public async Task<ActionResult> ToggleFavorite(int mediaId, [FromBody] FavorCreateDto dto)
    {
        try
        {
            var isFavorited = await _favorService.ToggleFavoriteAsync(mediaId, dto.UserId);

            return Ok(new
            {
                IsFavorited = isFavorited,
                Message = isFavorited ? "Added to favorites" : "Removed from favorites"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite for media {MediaId}", mediaId);
            return StatusCode(500, new { error = "Failed to toggle favorite" });
        }
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLower() switch
        {
            ".mp4" => "video/mp4",
            ".mkv" => "video/x-matroska",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".webm" => "video/webm",
            ".flv" => "video/x-flv",
            ".wmv" => "video/x-ms-wmv",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".ogg" => "audio/ogg",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".vtt" => "text/vtt",
            ".srt" => "text/srt",
            _ => "application/octet-stream"
        };
    }
}
