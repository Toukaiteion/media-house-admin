using MediaHouse.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Data;

public class AppUserRepository : Repository<AppUser>, Interfaces.IAppUserRepository
{
    public AppUserRepository(MediaHouseDbContext context, ILogger<AppUserRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Favorites)
            .Include(u => u.PlayRecords)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        return await _dbSet
            .Include(u => u.Favorites)
            .Include(u => u.PlayRecords)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
