using System.Collections.Concurrent;
using WatchSyncMovie.Server.Models;

namespace WatchSyncMovie.Server.Services;

public class RoomService : IRoomService
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly ConcurrentDictionary<string, User> _connectionUsers = new();

    public Task<Room?> GetRoomAsync(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return Task.FromResult(room);
    }

    public Task<Room> CreateRoomAsync(Room room)
    {
        _rooms[room.Id] = room;
        return Task.FromResult(room);
    }

    public Task<User?> GetUserByConnectionIdAsync(string connectionId)
    {
        _connectionUsers.TryGetValue(connectionId, out var user);
        return Task.FromResult(user);
    }

    public async Task AddUserToRoomAsync(string roomId, User user)
    {
        var room = await GetRoomAsync(roomId);
        if (room != null)
        {
            // Remove user from previous room if exists
            if (_connectionUsers.TryGetValue(user.ConnectionId, out var existingUser) && 
                existingUser.RoomId != null)
            {
                await RemoveUserFromRoomAsync(existingUser.RoomId, user.ConnectionId);
            }

            room.Users.Add(user);
            _connectionUsers[user.ConnectionId] = user;
        }
    }

    public async Task RemoveUserFromRoomAsync(string roomId, string connectionId)
    {
        var room = await GetRoomAsync(roomId);
        if (room != null)
        {
            room.Users.RemoveAll(u => u.ConnectionId == connectionId);
            _connectionUsers.TryRemove(connectionId, out _);

            // If room is empty, remove it
            if (room.Users.Count == 0)
            {
                _rooms.TryRemove(roomId, out _);
            }
            // If host left, assign new host
            else if (room.HostConnectionId == connectionId && room.Users.Count > 0)
            {
                var newHost = room.Users.First();
                newHost.IsHost = true;
                room.HostConnectionId = newHost.ConnectionId;
            }
        }
    }

    public async Task UpdateRoomStateAsync(string roomId, TimeSpan position, bool? isPlaying)
    {
        var room = await GetRoomAsync(roomId);
        if (room != null)
        {
            room.CurrentPosition = position;
            if (isPlaying.HasValue)
            {
                room.IsPlaying = isPlaying.Value;
            }
            room.LastUpdated = DateTime.UtcNow;
        }
    }

    public async Task SetCurrentMovieAsync(string roomId, string movieId)
    {
        var room = await GetRoomAsync(roomId);
        if (room != null)
        {
            room.CurrentMovieId = movieId;
            room.CurrentPosition = TimeSpan.Zero;
            room.IsPlaying = false;
            room.LastUpdated = DateTime.UtcNow;
        }
    }

    public Task<List<Room>> GetActiveRoomsAsync()
    {
        var activeRooms = _rooms.Values
            .Where(r => r.Users.Count > 0)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
        
        return Task.FromResult(activeRooms);
    }
} 