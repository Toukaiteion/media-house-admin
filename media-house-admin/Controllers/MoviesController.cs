using Microsoft.AspNetCore.Mvc;
using MediaHouse.Entities;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediaFileService _mediaFileService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(
        IMediaFileService mediaFileService,
        ILogger<MoviesController> logger)
    {
        _mediaFileService = mediaFileService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> GetMovies([FromQuery] int? libraryId = null)
    {
        // TODO: Implement with proper service
        return Ok(new List<MovieDto>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieDto>> GetMovie(int id)
    {
        // TODO: Implement with proper service
        return NotFound();
    }
}
