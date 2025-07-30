using WatchSyncMovie.Server.Models;

namespace WatchSyncMovie.Server.Services;

public interface IMovieService
{
    Task<Movie?> GetMovieAsync(string movieId);
    Task<Movie> CreateMovieAsync(Movie movie);
    Task<List<Movie>> GetMoviesAsync();
    Task<bool> DeleteMovieAsync(string movieId);
    Task<Movie> CreateMovieFromUrlAsync(string url, string title, string? description = null);
    Task<Movie> CreateMovieFromFileAsync(IFormFile file, string title, string? description = null);
    Task<List<Movie>> ScanVideosDirectoryAsync();
    void SetBaseUrl(string baseUrl);
} 