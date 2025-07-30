using WatchSyncMovie.Server.Models;

namespace WatchSyncMovie.Server.Services;

public interface IRoomService
{
    Task<Room?> GetRoomAsync(string roomId);
    Task<Room> CreateRoomAsync(Room room);
    Task<User?> GetUserByConnectionIdAsync(string connectionId);
    Task AddUserToRoomAsync(string roomId, User user);
    Task RemoveUserFromRoomAsync(string roomId, string connectionId);
    Task UpdateRoomStateAsync(string roomId, TimeSpan position, bool? isPlaying);
    Task SetCurrentMovieAsync(string roomId, string movieId);
    Task<List<Room>> GetActiveRoomsAsync();
} 