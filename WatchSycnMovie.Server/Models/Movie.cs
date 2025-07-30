namespace WatchSyncMovie.Server.Models;

public class Movie
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public MovieType Type { get; set; } = MovieType.Url;
}

public enum MovieType
{
    Url,
    Upload
} 