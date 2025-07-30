using System.Collections.Concurrent;
using WatchSyncMovie.Server.Models;

namespace WatchSyncMovie.Server.Services;

public class MovieService : IMovieService
{
    private readonly ConcurrentDictionary<string, Movie> _movies = new();
    private string _baseUrl = "http://localhost:5172"; // Default, will be updated from HttpContext

    public async Task<Movie?> GetMovieAsync(string movieId)
    {
        // First check in memory (uploaded/URL movies)
        if (_movies.TryGetValue(movieId, out var movie))
        {
            return movie;
        }

        // If not found in memory, check scanned files from wwwroot
        var scannedMovies = await ScanVideosDirectoryAsync();
        var scannedMovie = scannedMovies.FirstOrDefault(m => m.Id == movieId);
        
        return scannedMovie;
    }

    public Task<Movie> CreateMovieAsync(Movie movie)
    {
        _movies[movie.Id] = movie;
        return Task.FromResult(movie);
    }

    public async Task<List<Movie>> GetMoviesAsync()
    {
        // Get movies from memory (URL and registered uploads)
        var memoryMovies = _movies.Values.ToList();
        
        // Scan wwwroot/videos directory for additional files
        var scannedMovies = await ScanVideosDirectoryAsync();
        
        // Merge and remove duplicates (prefer memory movies over scanned ones)
        var allMovies = new List<Movie>();
        allMovies.AddRange(memoryMovies);
        
        // Add scanned movies that are not already in memory
        foreach (var scannedMovie in scannedMovies)
        {
            if (!memoryMovies.Any(m => m.VideoUrl.Equals(scannedMovie.VideoUrl, StringComparison.OrdinalIgnoreCase)))
            {
                allMovies.Add(scannedMovie);
            }
        }
        
        return allMovies.OrderByDescending(m => m.CreatedAt).ToList();
    }

    public async Task<bool> DeleteMovieAsync(string movieId)
    {
        if (_movies.TryGetValue(movieId, out var movie))
        {
            // If it's an uploaded file, delete the physical file
            if (movie.Type == MovieType.Upload && !string.IsNullOrEmpty(movie.VideoUrl))
            {
                try
                {
                    var fileName = Path.GetFileName(movie.VideoUrl);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", fileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the deletion
                    Console.WriteLine($"Failed to delete file for movie {movieId}: {ex.Message}");
                }
            }
            
            var removed = _movies.TryRemove(movieId, out _);
            return await Task.FromResult(removed);
        }
        
        return await Task.FromResult(false);
    }

    public async Task<Movie> CreateMovieFromUrlAsync(string url, string title, string? description = null)
    {
        var movie = new Movie
        {
            Title = title,
            Description = description,
            VideoUrl = url,
            Type = MovieType.Url
        };

        return await CreateMovieAsync(movie);
    }

    public async Task<Movie> CreateMovieFromFileAsync(IFormFile file, string title, string? description = null)
    {
        // Validate file type
        var allowedExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".mkv" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException($"File type {fileExtension} is not supported. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");
        
        // Ensure directory exists
        Directory.CreateDirectory(uploadsFolder);
        
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Create movie with absolute URL
        var videoUrl = $"{_baseUrl}/videos/{fileName}";
        var movie = new Movie
        {
            Title = title,
            Description = description,
            VideoUrl = videoUrl,
            Type = MovieType.Upload,
            Duration = TimeSpan.Zero // Could be extracted from video metadata if needed
        };

        return await CreateMovieAsync(movie);
    }

    public async Task<List<Movie>> ScanVideosDirectoryAsync()
    {
        var scannedMovies = new List<Movie>();
        var videosPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");
        
        if (!Directory.Exists(videosPath))
        {
            return scannedMovies;
        }

        try
        {
            var allowedExtensions = new[] { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".mkv" };
            var videoFiles = Directory.GetFiles(videosPath)
                .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToList();

            foreach (var filePath in videoFiles)
            {
                var fileName = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);
                
                // Create a title from filename (remove extension and GUID if present)
                var title = Path.GetFileNameWithoutExtension(fileName);
                
                // If filename is a GUID, try to create a more user-friendly title
                if (Guid.TryParse(title, out _))
                {
                    title = $"Video File {Path.GetExtension(fileName).TrimStart('.').ToUpper()}";
                }
                else
                {
                    // Replace underscores and dashes with spaces, capitalize first letter
                    title = title.Replace("_", " ").Replace("-", " ");
                    if (!string.IsNullOrEmpty(title))
                    {
                        title = char.ToUpper(title[0]) + title.Substring(1);
                    }
                }

                var movie = new Movie
                {
                    Id = $"wwwroot_{fileName.Replace(".", "_").Replace(" ", "_").Replace("-", "_")}", // Unique ID for file-based movies
                    Title = title,
                    Description = $"Video file from server ({FormatFileSize(fileInfo.Length)})",
                    VideoUrl = $"{_baseUrl}/videos/{fileName}",
                    Type = MovieType.Upload,
                    CreatedAt = fileInfo.CreationTimeUtc,
                    Duration = TimeSpan.Zero // Could be extracted with video libraries if needed
                };

                scannedMovies.Add(movie);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - return empty list instead
            Console.WriteLine($"Error scanning videos directory: {ex.Message}");
        }

        return await Task.FromResult(scannedMovies);
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }

    public void SetBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
    }
} 