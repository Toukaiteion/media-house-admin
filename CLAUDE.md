# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MediaHouse Admin is a .NET 10.0 ASP.NET Core Web API for managing media libraries (movies and TV shows). It provides REST APIs for library management, media scanning, and playback tracking.

## Build and Run

```bash
# Build the project
dotnet build

# Run the application
dotnet run --project media-house-admin

# Run with specific configuration
dotnet run --project media-house-admin --configuration Release
```

The application runs on `http://localhost:5000` or `https://localhost:5001` by default (configurable in `Properties/launchSettings.json`).

## Architecture

### Layered Structure

- **Controllers** (`media-house-admin/Controllers/`): REST API endpoints
- **Services** (`media-house-admin/Services/`): Business logic implementations
- **Interfaces** (`media-house-admin/Interfaces/`): Service contracts
- **Entities** (`media-house-admin/Entities/`): Domain models (Movie, TVShow, Season, Episode, MediaLibrary, etc.)
- **DTOs** (`media-house-admin/DTOs/`): Data transfer objects for API requests/responses
- **Data** (`media-house-admin/Data/`): Database context and initialization
- **BackgroundJobs** (`media-house-admin/BackgroundJobs/`): Quartz.NET scheduled jobs

### Database

Uses Entity Framework Core with SQLite. Database file: `media-house-admin/mediahouse.db`

Key entities and relationships:
- MediaLibrary → Movies (one-to-many, cascade delete)
- MediaLibrary → TVShows (one-to-many, cascade delete)
- TVShow → Seasons (one-to-many, cascade delete)
- Season → Episodes (one-to-many, cascade delete)
- Movie → MediaFile (one-to-one, cascade delete)
- Episode → MediaFile (one-to-one, cascade delete)
- Movie/TVShow/Episode → NfoMetadata (one-to-one, cascade delete)
- PlaybackProgress tracks user playback state

### Background Jobs

Uses Quartz.NET for scheduling:
- **ScanJob**: Incremental library scans (triggered via QuartzService.ScheduleIncrementalScan)
- **ConsistencyCheckJob**: System consistency checks (triggered via QuartzService.ScheduleConsistencyCheck)
- QuartzService is registered as a hosted service and manages job scheduling

### Key Services

- **LibraryService**: CRUD operations for media libraries
- **ScanService**: Full and incremental scanning of library directories
- **MetadataService**: Parsing NFO metadata files and extracting media metadata
- **ConsistencyService**: Checking database consistency with file system
- **PlaybackService**: Tracking user playback progress
- **MediaFileService**: Media file operations and serving

### API Endpoints

- `/api/libraries`: Library management (create, update, delete, trigger scans)
- `/api/movies`: Movie operations
- `/api/tvshows`: TV show operations (includes seasons/episodes endpoints)
- `/api/media`: Media file serving (`GET /api/media/file?path=...`)
- `/api/playback`: Playback progress tracking
- `/health`: Health check endpoint

## JSON Configuration

Controllers use camelCase JSON naming policy and indented output for readability.

## CORS

The application uses a permissive CORS policy allowing any origin, method, and header.

## Dependencies

Key NuGet packages:
- Microsoft.EntityFrameworkCore.Sqlite (9.0.0)
- Quartz (3.14.0) & Quartz.Extensions.Hosting (3.14.0)
- MediaInfo.Wrapper (26.1.0) - for media file metadata extraction

## Development Notes

- The codebase has several "TODO" placeholders for unimplemented features
- NFO file parsing and media metadata extraction are stub implementations
- File system scanning logic is partially implemented
- Database is initialized on application startup via DatabaseService.InitializeDatabaseAsync()

## Docker Support

The project can be containerized using the included Dockerfile:

```bash
# Build the Docker image
docker build -t media-house-admin .

# Run the container
docker run -p 5000:5000 media-house-admin
```

## Database Initialization

The database is initialized on application startup via `DatabaseService.InitializeDatabaseAsync()`.
The `db_init.sql` file contains SQL scripts for manual database setup if needed.
Database file location: `media-house-admin/mediahouse.db`

To manually reset the database:
1. Stop the application
2. Delete `mediahouse.db`
3. Restart the application (it will auto-initialize)

## Service Registration

Services are registered in `Program.cs` using dependency injection:
- Scoped services (e.g., LibraryService, ScanService) for per-request lifetime
- Singleton services (e.g., QuartzService) for application lifetime
- Hosted services (e.g., QuartzService) for background processing

## Testing

The project does not currently have an automated test suite. Consider adding tests using:
- xUnit or NUnit for unit tests
- Microsoft.AspNetCore.Mvc.Testing for integration tests