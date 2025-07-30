using Microsoft.AspNetCore.Mvc;
using WatchSyncMovie.Server.Models;
using WatchSyncMovie.Server.Services;

namespace WatchSyncMovie.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    private void UpdateBaseUrl()
    {
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        _movieService.SetBaseUrl(baseUrl);
    }

    [HttpGet]
    public async Task<ActionResult<List<Movie>>> GetMovies()
    {
        UpdateBaseUrl();
        var movies = await _movieService.GetMoviesAsync();
        return Ok(movies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Movie>> GetMovie(string id)
    {
        var movie = await _movieService.GetMovieAsync(id);
        if (movie == null)
        {
            return NotFound();
        }
        return Ok(movie);
    }

    [HttpPost]
    public async Task<ActionResult<Movie>> CreateMovie([FromBody] CreateMovieRequest request)
    {
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            VideoUrl = request.VideoUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            Duration = request.Duration,
            Type = request.Type
        };

        var createdMovie = await _movieService.CreateMovieAsync(movie);
        return CreatedAtAction(nameof(GetMovie), new { id = createdMovie.Id }, createdMovie);
    }

    [HttpPost("from-url")]
    public async Task<ActionResult<Movie>> CreateMovieFromUrl([FromBody] CreateMovieFromUrlRequest request)
    {
        try
        {
            var movie = await _movieService.CreateMovieFromUrlAsync(request.Url, request.Title, request.Description);
            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating movie from URL: {ex.Message}");
        }
    }

    [HttpPost("upload")]
    [RequestSizeLimit(2047*1024*1024)]
    public async Task<ActionResult<Movie>> UploadMovie([FromForm] UploadMovieRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            UpdateBaseUrl();
            var movie = await _movieService.CreateMovieFromFileAsync(request.File, request.Title, request.Description);
            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error uploading movie: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMovie(string id)
    {
        var deleted = await _movieService.DeleteMovieAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}

public class CreateMovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public TimeSpan Duration { get; set; }
    public MovieType Type { get; set; } = MovieType.Url;
}

public class CreateMovieFromUrlRequest
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UploadMovieRequest
{
    public IFormFile File { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
} 