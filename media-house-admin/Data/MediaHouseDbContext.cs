using MediaHouse.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaHouse.Data;

public class MediaHouseDbContext(DbContextOptions<MediaHouseDbContext> options) : DbContext(options)
{
    public DbSet<MediaLibrary> MediaLibraries { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<TVShow> TVShows { get; set; }
    public DbSet<Season> Seasons { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<MediaFile> MediaFiles { get; set; }
    public DbSet<MediaImgs> MediaImgs { get; set; }
    public DbSet<SystemSyncLog> SystemSyncLogs { get; set; }
    public DbSet<PlayRecord> PlayRecords { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<MediaTag> MediaTags { get; set; }
    public DbSet<MyFavor> MyFavors { get; set; }
    public DbSet<MediaStaff> MediaStaffs { get; set; }
    public DbSet<MediaItem> MediaItems { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MediaLibrary -> MediaItems
        modelBuilder.Entity<MediaLibrary>()
            .HasMany(m => m.MediaItems)
            .WithOne(mi => mi.Library)
            .OnDelete(DeleteBehavior.Cascade);

        // MediaItem -> Movie (1:1)
        modelBuilder.Entity<MediaItem>()
            .HasOne(mi => mi.Movie)
            .WithOne(m => m.MediaItem)
            .HasForeignKey<Movie>(m => m.MediaItemId)
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

        // Episode -> TVShow (for navigation)
        modelBuilder.Entity<Episode>()
            .HasOne(e => e.TVShow)
            .WithMany()
            .HasForeignKey(e => e.TVShowId)
            .OnDelete(DeleteBehavior.Restrict);

        // Movie -> MediaFile (one-to-one via MovieId - legacy)
        modelBuilder.Entity<MediaFile>()
            .HasOne(mf => mf.Movie)
            .WithMany()
            .HasForeignKey(mf => mf.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // Episode -> MediaFile (one-to-one via EpisodeId - legacy)
        modelBuilder.Entity<MediaFile>()
            .HasOne(mf => mf.Episode)
            .WithMany()
            .HasForeignKey(mf => mf.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // MediaImgs -> Movie relationship
        modelBuilder.Entity<MediaImgs>()
            .HasOne(mi => mi.Movie)
            .WithMany(m => m.MediaImgs)
            .HasForeignKey(mi => mi.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        // MediaImgs -> Episode relationship
        modelBuilder.Entity<MediaImgs>()
            .HasOne(mi => mi.Episode)
            .WithMany()
            .HasForeignKey(mi => mi.MediaId)
            .OnDelete(DeleteBehavior.Restrict);

        // SystemSyncLog -> MediaLibrary
        modelBuilder.Entity<SystemSyncLog>()
            .HasOne(sl => sl.MediaLibrary)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        // PlayRecord -> Movie (optional)
        modelBuilder.Entity<PlayRecord>()
            .HasOne(p => p.Movie)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        // PlayRecord -> Episode (optional)
        modelBuilder.Entity<PlayRecord>()
            .HasOne(p => p.Episode)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        // AppUser relationships
        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.Favorites)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.PlayRecords)
            .WithOne()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // MyFavor -> MediaLibrary
        modelBuilder.Entity<MyFavor>()
            .HasOne(f => f.MediaLibrary)
            .WithMany()
            .HasForeignKey(f => f.MediaLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        // MediaTag -> MediaLibrary
        modelBuilder.Entity<MediaTag>()
            .HasOne(t => t.MediaLibrary)
            .WithMany()
            .HasForeignKey(t => t.MediaLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        // MediaTag -> Tag relationship
        modelBuilder.Entity<MediaTag>()
            .HasOne(mt => mt.Tag)
            .WithMany(t => t.MediaTags)
            .HasForeignKey(mt => mt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // MediaStaff -> Staff
        modelBuilder.Entity<MediaStaff>()
            .HasOne(ms => ms.Staff)
            .WithMany(s => s.MediaStaffs)
            .HasForeignKey(ms => ms.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        modelBuilder.Entity<MediaFile>()
            .HasIndex(mf => mf.Path)
            .IsUnique();

        modelBuilder.Entity<PlayRecord>()
            .HasIndex(p => new { p.UserId, p.MediaType, p.MediaId });

        modelBuilder.Entity<PlayRecord>()
            .HasIndex(p => p.LastPlayTime);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Composite key for MediaTag - matches SQL (lib_id, media_id, tag_id)
        modelBuilder.Entity<MediaTag>()
            .HasKey(t => new { t.MediaLibraryId, t.MediaId, t.TagId });

        // Enum conversions
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

        modelBuilder.Entity<MediaFile>()
            .Property(mf => mf.MediaType)
            .HasConversion<string>();

        modelBuilder.Entity<PlayRecord>()
            .Property(p => p.MediaType)
            .HasConversion<string>();

        modelBuilder.Entity<MediaTag>()
            .Property(t => t.MediaType)
            .HasConversion<string>();

        modelBuilder.Entity<MediaStaff>()
            .Property(ms => ms.MediaType)
            .HasConversion<string>();

        modelBuilder.Entity<MediaStaff>()
            .Property(ms => ms.RoleType)
            .HasConversion<string>();

        modelBuilder.Entity<MediaImgs>()
            .Property(mi => mi.MediaType)
            .HasConversion<string>();

        modelBuilder.Entity<MediaImgs>()
            .Property(mi => mi.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Staff>()
            .Property(s => s.Country)
            .HasConversion<string>();
    }
}
