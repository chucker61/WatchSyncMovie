namespace WatchSyncMovie.Client.Models;

public class Movie
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public MovieType Type { get; set; }
}

public enum MovieType
{
    Url,
    Upload
} 