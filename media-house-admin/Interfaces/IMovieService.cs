using MediaHouse.DTOs;

namespace MediaHouse.Interfaces;

public interface IMovieService
{
    Task<(List<MovieDto> Movies, int TotalCount)> GetMoviesAsync(MovieQueryDto query);
    Task<bool> DeleteMovieAsync(int mediaId);
}
