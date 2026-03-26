using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediaHouse.Data;

namespace MediaHouse.Data;

public class DatabaseService
{
    private readonly MediaHouseDbContext _context;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(MediaHouseDbContext context, ILogger<DatabaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            // Ensure database is created and migrations applied
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database");
            throw;
        }
    }

    public async Task<bool> DatabaseExistsAsync()
    {
        return await _context.Database.CanConnectAsync();
    }

    public async Task MigrateDatabaseAsync()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migration completed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate database");
            throw;
        }
    }
}
