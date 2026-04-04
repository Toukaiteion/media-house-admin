using Microsoft.AspNetCore.Mvc;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController(
    IMediaService mediaService,
    ILogger<MediaController> logger) : ControllerBase
{
    private readonly IMediaService _mediaService = mediaService;
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
