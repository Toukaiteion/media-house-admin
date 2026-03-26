using MediaHouse.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Data;

public class MediaHouseDbContext : DbContext
{
    public DbSet<MediaLibrary> MediaLibraries { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<TVShow> TVShows { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<MediaFile> MediaFiles { get; set; }
    public DbSet<NfoMetadata> NfoMetadata { get; set; }
    public DbSet<SystemSyncLog> SystemSyncLogs { get; set; }
    public DbSet<PlaybackProgress> PlaybackProgresses { get; set; }

    public MediaHouseDbContext(DbContextOptions<MediaHouseDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MediaLibrary
        modelBuilder.Entity<MediaLibrary>()
            .HasMany(m => m.Movies)
            .WithOne(m => m.MediaLibrary)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MediaLibrary>()
            .HasMany(m => m.TVShows)
            .WithOne(t => t.MediaLibrary)
            .OnDelete(DeleteBehavior.Cascade);

        // TVShow -> Seasons
        modelBuilder.Entity<TVShow>()
            .HasMany(t => t.Seasons)
            .WithOne(s => s.TVShow)
            .OnDelete(DeleteBehavior.Cascade);

        // Season -> Episodes
        modelBuilder.Entity<Season>()
            .HasMany(s => s.Episodes)
            .WithOne(e => e.Season)
            .OnDelete(DeleteBehavior.Cascade);

        // Movie -> MediaFile (one-to-one)
        modelBuilder.Entity<Movie>()
            .HasOne(m => m.MediaFile)
            .WithOne()
            .HasForeignKey<MediaFile>(mf => mf.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // Episode -> MediaFile (one-to-one)
        modelBuilder.Entity<Episode>()
            .HasOne(e => e.MediaFile)
            .WithOne()
            .HasForeignKey<MediaFile>(mf => mf.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Movie -> NfoMetadata (one-to-one)
        modelBuilder.Entity<Movie>()
            .HasOne(m => m.Metadata)
            .WithOne()
            .HasForeignKey<NfoMetadata>(nm => nm.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // TVShow -> NfoMetadata (one-to-one)
        modelBuilder.Entity<TVShow>()
            .HasOne(t => t.Metadata)
            .WithOne()
            .HasForeignKey<NfoMetadata>(nm => nm.TVShowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Episode -> NfoMetadata (one-to-one)
        modelBuilder.Entity<Episode>()
            .HasOne(e => e.Metadata)
            .WithOne()
            .HasForeignKey<NfoMetadata>(nm => nm.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // SystemSyncLog
        modelBuilder.Entity<SystemSyncLog>()
            .HasOne(sl => sl.MediaLibrary)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        // PlaybackProgress
        modelBuilder.Entity<PlaybackProgress>()
            .HasOne(p => p.Movie)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlaybackProgress>()
            .HasOne(p => p.Episode)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        modelBuilder.Entity<MediaFile>()
            .HasIndex(mf => mf.FilePath)
            .IsUnique();

        modelBuilder.Entity<PlaybackProgress>()
            .HasIndex(p => new { p.UserId, p.MovieId });

        modelBuilder.Entity<PlaybackProgress>()
            .HasIndex(p => new { p.UserId, p.EpisodeId });

        // Enum conversion
        modelBuilder.Entity<MediaLibrary>()
            .Property(m => m.Type)
            .HasConversion<string>();

        modelBuilder.Entity<MediaLibrary>()
            .Property(m => m.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SystemSyncLog>()
            .Property(s => s.SyncType)
            .HasConversion<string>();

        modelBuilder.Entity<SystemSyncLog>()
            .Property(s => s.Status)
            .HasConversion<string>();
    }
}
