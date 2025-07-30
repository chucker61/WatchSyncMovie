namespace WatchSyncMovie.Client.Models;

public class Room
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? CurrentMovieId { get; set; }
    public TimeSpan CurrentPosition { get; set; }
    public bool IsPlaying { get; set; }
    public string? HostConnectionId { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<User> Users { get; set; } = new();
    public DateTime CreatedAt { get; set; }
} 