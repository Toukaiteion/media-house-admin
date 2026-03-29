using Microsoft.AspNetCore.Mvc;
using MediaHouse.DTOs;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TVShowsController : ControllerBase
{
    private readonly ILogger<TVShowsController> _logger;

    public TVShowsController(ILogger<TVShowsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<TVShowDto>>> GetTVShows([FromQuery] string? libraryId = null)
    {
        // TODO: Implement with proper service
        return Ok(new List<TVShowDto>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TVShowDto>> GetTVShow(string id)
    {
        // TODO: Implement with proper service
        return NotFound();
    }

    [HttpGet("{id}/seasons")]
    public async Task<ActionResult<List<SeasonDto>>> GetSeasons(string id)
    {
        // TODO: Implement with proper service
        return Ok(new List<SeasonDto>());
    }

    [HttpGet("{tvShowId}/seasons/{seasonId}/episodes")]
    public async Task<ActionResult<List<EpisodeDto>>> GetEpisodes(string tvShowId, string seasonId)
    {
        // TODO: Implement with proper service
        return Ok(new List<EpisodeDto>());
    }
}
