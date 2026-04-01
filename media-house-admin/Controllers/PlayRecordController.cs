using Microsoft.AspNetCore.Mvc;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;
using MediaHouse.Data.Entities;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayRecordController : ControllerBase
{
    private readonly IPlayRecordService _playRecordService;
    private readonly ILogger<PlayRecordController> _logger;

    public PlayRecordController(
        IPlayRecordService playRecordService,
        ILogger<PlayRecordController> logger)
    {
        _playRecordService = playRecordService;
        _logger = logger;
    }

    [HttpGet("url")]
    public async Task<ActionResult<PlayRecordUrlDto>> GetPlaybackUrl([FromQuery] int mediaId, [FromQuery] string mediaType)
    {
        try
        {
            var url = await _playRecordService.GetPlaybackUrlAsync(mediaId, mediaType);
            return Ok(new PlayRecordUrlDto
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

    [HttpPost("progress")]
    public async Task<ActionResult> UpdatePlaybackProgress([FromBody] UpdatePlayRecordDto dto)
    {
        try
        {
            await _playRecordService.UpdatePlaybackProgressAsync(dto.UserId, dto.MediaLibraryId, dto.MediaType, dto.MediaId, dto.PositionSeconds);
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
            await _playRecordService.MarkAsCompletedAsync(dto.UserId, dto.MediaLibraryId, dto.MediaType, dto.MediaId);
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
