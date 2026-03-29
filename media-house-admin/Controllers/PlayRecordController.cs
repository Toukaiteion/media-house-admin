using Microsoft.AspNetCore.Mvc;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;
using MediaHouse.Entities;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayRecordController : ControllerBase
{
    private readonly IPlaybackService _playbackService;
    private readonly ILogger<PlayRecordController> _logger;

    public PlayRecordController(
        IPlaybackService playbackService,
        ILogger<PlayRecordController> logger)
    {
        _playbackService = playbackService;
        _logger = logger;
    }

    [HttpGet("url")]
    public async Task<ActionResult<PlaybackUrlDto>> GetPlaybackUrl([FromQuery] string mediaId, [FromQuery] string mediaType)
    {
        try
        {
            var url = await _playbackService.GetPlaybackUrlAsync(mediaId, mediaType);
            return Ok(new PlaybackUrlDto
            {
                Url = url,
                MimeType = GetMimeType(mediaType),
                CanDirectPlay = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playback URL for media {MediaId}", mediaId);
            return StatusCode(500, new { error = "Failed to get playback URL" });
        }
    }

    [HttpGet("progress")]
    public async Task<ActionResult<PlayRecordDto>> GetPlaybackProgress(
        [FromQuery] string userId,
        [FromQuery] string mediaLibraryId,
        [FromQuery] MediaType mediaType,
        [FromQuery] string mediaId)
    {
        try
        {
            var progress = await _playbackService.GetPlaybackProgressAsync(userId, mediaLibraryId, mediaType, mediaId);
            if (progress == null)
            {
                return NotFound(new { error = "No playback progress found" });
            }

            return Ok(MapToDto(progress));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playback progress");
            return StatusCode(500, new { error = "Failed to get playback progress" });
        }
    }

    [HttpPost("progress")]
    public async Task<ActionResult> UpdatePlaybackProgress([FromBody] UpdatePlayRecordDto dto)
    {
        try
        {
            await _playbackService.UpdatePlaybackProgressAsync(dto.UserId, dto.MediaLibraryId, dto.MediaType, dto.MediaId, dto.PositionSeconds);
            return Ok(new { message = "Progress updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating playback progress");
            return StatusCode(500, new { error = "Failed to update progress" });
        }
    }

    [HttpPut("progress/complete")]
    public async Task<ActionResult> MarkAsCompleted([FromBody] UpdatePlayRecordDto dto)
    {
        try
        {
            await _playbackService.MarkAsCompletedAsync(dto.UserId, dto.MediaLibraryId, dto.MediaType, dto.MediaId);
            return Ok(new { message = "Playback marked as completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking playback as completed");
            return StatusCode(500, new { error = "Failed to mark as completed" });
        }
    }

    private static string GetMimeType(string mediaType)
    {
        return mediaType.ToLower() switch
        {
            "movie" or "episode" => "video/mp4",
            _ => "application/octet-stream"
        };
    }

    private static PlayRecordDto MapToDto(PlayRecord progress)
    {
        return new PlayRecordDto
        {
            Id = progress.Id,
            UserId = progress.UserId,
            MediaLibraryId = progress.MediaLibraryId,
            MediaType = progress.MediaType,
            MediaId = progress.MediaId,
            PositionMs = progress.PositionMs,
            IsFinished = progress.IsFinished,
            LastPlayTime = progress.LastPlayTime,
            CreatedAt = progress.CreatedAt,
            UpdatedAt = progress.UpdatedAt
        };
    }
}
