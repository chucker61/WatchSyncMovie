namespace WatchSyncMovie.Server.Models;

public class SyncEvent
{
    public string Type { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PlayEvent
{
    public TimeSpan Position { get; set; }
}

public class PauseEvent
{
    public TimeSpan Position { get; set; }
}

public class SeekEvent
{
    public TimeSpan Position { get; set; }
}

public class ChangeMovieEvent
{
    public string MovieId { get; set; } = string.Empty;
} 