using Microsoft.AspNetCore.Mvc;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<List<TVShowDto>>> GetTVShows([FromQuery] int? libraryId = null)
    {
        // TODO: Implement with proper service
        return Ok(new List<TVShowDto>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TVShowDto>> GetTVShow(int id)
    {
        // TODO: Implement with proper service
        return NotFound();
    }

    [HttpGet("{id}/seasons")]
    public async Task<ActionResult<List<SeasonDto>>> GetSeasons(int id)
    {
        // TODO: Implement with proper service
        return Ok(new List<SeasonDto>());
    }

    [HttpGet("{tvShowId}/seasons/{seasonId}/episodes")]
    public async Task<ActionResult<List<EpisodeDto>>> GetEpisodes(int tvShowId, int seasonId)
    {
        // TODO: Implement with proper service
        return Ok(new List<EpisodeDto>());
    }
}
