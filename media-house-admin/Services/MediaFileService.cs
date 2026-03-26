using MediaHouse.Entities;
using MediaHouse.Interfaces;
using MediaHouse.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaHouse.Services;

public class MediaFileService : IMediaFileService
{
    private readonly MediaHouseDbContext _context;
    private readonly ILogger<MediaFileService> _logger;

    public MediaFileService(MediaHouseDbContext context, ILogger<MediaFileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MediaFile?> GetMediaFileByPathAsync(string path)
    {
        return await _context.MediaFiles
            .FirstOrDefaultAsync(mf => mf.FilePath == path);
    }

    public async Task<MediaFile?> GetMediaFileByIdAsync(int id)
    {
        return await _context.MediaFiles.FindAsync(id);
    }

    public async Task<MediaFile> CreateMediaFileAsync(string filePath, int? movieId = null, int? episodeId = null)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var fileInfo = new FileInfo(filePath);

        var mediaFile = new MediaFile
        {
            FileName = fileInfo.Name,
            FilePath = filePath,
            ContainerFormat = fileInfo.Extension.TrimStart('.'),
            FileSize = fileInfo.Length,
            LastModified = fileInfo.LastWriteTimeUtc,
            MovieId = movieId,
            EpisodeId = episodeId
        };

        // TODO: Extract media info using MediaInfo or FFmpeg

        _context.MediaFiles.Add(mediaFile);
        await _context.SaveChangesAsync();

        return mediaFile;
    }

    public async Task<MediaFile?> UpdateMediaFileAsync(int id, MediaFile updatedFile)
    {
        var mediaFile = await _context.MediaFiles.FindAsync(id);
        if (mediaFile == null) return null;

        mediaFile.FileName = updatedFile.FileName;
        mediaFile.FilePath = updatedFile.FilePath;
        mediaFile.ContainerFormat = updatedFile.ContainerFormat;
        mediaFile.VideoCodec = updatedFile.VideoCodec;
        mediaFile.AudioCodec = updatedFile.AudioCodec;
        mediaFile.Width = updatedFile.Width;
        mediaFile.Height = updatedFile.Height;
        mediaFile.Duration = updatedFile.Duration;
        mediaFile.FileSize = updatedFile.FileSize;
        mediaFile.LastModified = updatedFile.LastModified;
        mediaFile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return mediaFile;
    }

    public async Task<bool> DeleteMediaFileAsync(int id)
    {
        var mediaFile = await _context.MediaFiles.FindAsync(id);
        if (mediaFile == null) return false;

        _context.MediaFiles.Remove(mediaFile);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<MediaFile>> GetMediaFilesForLibraryAsync(int libraryId)
    {
        // TODO: Implement proper query
        return await _context.MediaFiles.ToListAsync();
    }
}
