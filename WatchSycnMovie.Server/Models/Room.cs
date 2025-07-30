namespace WatchSyncMovie.Server.Models;

public class Room
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? CurrentMovieId { get; set; }
    public TimeSpan CurrentPosition { get; set; } = TimeSpan.Zero;
    public bool IsPlaying { get; set; } = false;
    public string? HostConnectionId { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<User> Users { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 