using Microsoft.AspNetCore.Mvc;
using MediaHouse.Data.Entities;
using MediaHouse.DTOs;
using MediaHouse.Interfaces;

namespace MediaHouse.Controllers;

[ApiController]
[Route("api/actors")]
public class ActorsController(IActorService actorService, ILogger<ActorsController> logger) : ControllerBase
{
    private readonly IActorService _actorService = actorService;
    private readonly ILogger<ActorsController> _logger = logger;

    [HttpGet]
    public async Task<ActionResult> GetActors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var (actors, totalCount) = await _actorService.GetActorsAsync(page, pageSize);
            return Ok(new
            {
                Actors = actors.Select(a => new StaffDto
                {
                    Id = a.Id.ToString(),
                    Name = a.Name,
                    AvatarPath = a.AvatarPath
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching actors");
            return StatusCode(500, new { error = "Failed to fetch actors" });
        }
    }
}
