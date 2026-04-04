using MediaHouse.Data;
using MediaHouse.Data.Entities;
using MediaHouse.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Services;

public class ActorService(MediaHouseDbContext context, ILogger<ActorService> logger) : IActorService
{
    private readonly MediaHouseDbContext _context = context;
    private readonly ILogger<ActorService> _logger = logger;

    public async Task<(List<Staff> Actors, int TotalCount)> GetActorsAsync(int page, int pageSize)
    {
        var query = _context.Staffs.AsQueryable();
        var totalCount = await query.CountAsync();

        var actors = await query
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (actors, totalCount);
    }
}
