using MediaHouse.Data;
using MediaHouse.Interfaces;
using MediaHouse.Services;
using MediaHouse.BackgroundJobs;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add SQLite database
builder.Services.AddDbContext<MediaHouseDbContext>(options =>
    options.UseSqlite("Data Source=mediahouse.db"));

// Register services
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<IMetadataService, MetadataService>();
builder.Services.AddScoped<IConsistencyService, ConsistencyService>();
builder.Services.AddScoped<IPlaybackService, PlaybackService>();
builder.Services.AddScoped<IMediaFileService, MediaFileService>();
builder.Services.AddScoped<DatabaseService>();

// Add Quartz.NET
builder.Services.AddQuartz(q =>
{
    // Default job factory
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Configure jobs if needed
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    await dbService.InitializeDatabaseAsync();
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseStaticFiles();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
