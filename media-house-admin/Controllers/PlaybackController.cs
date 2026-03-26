using Microsoft.AspNetCore.Mvc;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;
using MediaHouse.Entities;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaybackController : ControllerBase
{
    private readonly IPlaybackService _playbackService;
    private readonly ILogger<PlaybackController> _logger;

    public PlaybackController(
        IPlaybackService playbackService,
        ILogger<PlaybackController> logger)
    {
        _playbackService = playbackService;
        _logger = logger;
    }

    [HttpGet("url")]
    public async Task<ActionResult<PlaybackUrlDto>> GetPlaybackUrl([FromQuery] int mediaId, [FromQuery] string mediaType)
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
    public async Task<ActionResult<PlaybackProgressDto>> GetPlaybackProgress([FromQuery] string userId = "default", [FromQuery] int? movieId = null, [FromQuery] int? episodeId = null)
    {
        try
        {
            if (!movieId.HasValue && !episodeId.HasValue)
            {
                return BadRequest(new { error = "Either movieId or episodeId must be provided" });
            }

            var progress = await _playbackService.GetPlaybackProgressAsync(userId, movieId, episodeId);
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
    public async Task<ActionResult> UpdatePlaybackProgress([FromBody] UpdatePlaybackProgressDto dto, [FromQuery] string userId = "default")
    {
        try
        {
            await _playbackService.UpdatePlaybackProgressAsync(userId, dto.MovieId, dto.EpisodeId, dto.Position, dto.Duration);
            return Ok(new { message = "Progress updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating playback progress");
            return StatusCode(500, new { error = "Failed to update progress" });
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

    private static PlaybackProgressDto MapToDto(PlaybackProgress progress)
    {
        return new PlaybackProgressDto
        {
            Id = progress.Id,
            UserId = progress.UserId,
            MovieId = progress.MovieId,
            EpisodeId = progress.EpisodeId,
            Position = progress.Position,
            Duration = progress.Duration,
            LastPlayed = progress.LastPlayed,
            IsCompleted = progress.IsCompleted
        };
    }
}
