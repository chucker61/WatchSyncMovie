namespace WatchSyncMovie.Server.Models;

public class User
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? RoomId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsHost { get; set; } = false;
} 