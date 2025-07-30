using Microsoft.AspNetCore.SignalR;
using WatchSyncMovie.Server.Models;
using WatchSyncMovie.Server.Services;

namespace WatchSyncMovie.Server.Hubs;

public class MovieSyncHub : Hub
{
    private readonly IRoomService _roomService;
    private readonly IMovieService _movieService;

    public MovieSyncHub(IRoomService roomService, IMovieService movieService)
    {
        _roomService = roomService;
        _movieService = movieService;
    }

    public async Task JoinRoom(string roomId, string username, string? password = null)
    {
        var room = await _roomService.GetRoomAsync(roomId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Room not found");
            return;
        }

        if (!string.IsNullOrEmpty(room.Password) && room.Password != password)
        {
            await Clients.Caller.SendAsync("Error", "Invalid password");
            return;
        }

        var user = new User
        {
            ConnectionId = Context.ConnectionId,
            Username = username,
            RoomId = roomId,
            IsHost = room.Users.Count == 0
        };

        await _roomService.AddUserToRoomAsync(roomId, user);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);



        // Send current room state to the new user
        await Clients.Caller.SendAsync("RoomJoined", room);
        
        // Notify other users
        await Clients.OthersInGroup(roomId).SendAsync("UserJoined", user);

        // Send current movie state if playing
        if (!string.IsNullOrEmpty(room.CurrentMovieId))
        {
            var movie = await _movieService.GetMovieAsync(room.CurrentMovieId);
            if (movie != null)
            {
                await Clients.Caller.SendAsync("MovieChanged", movie);
                await Clients.Caller.SendAsync("SyncState", new
                {
                    Position = room.CurrentPosition,
                    IsPlaying = room.IsPlaying
                });
            }
        }
    }

    public async Task CreateRoom(string roomName, string username, string? password = null)
    {
        var room = new Room
        {
            Name = roomName,
            Password = password,
            HostConnectionId = Context.ConnectionId
        };

        var createdRoom = await _roomService.CreateRoomAsync(room);
        
        var user = new User
        {
            ConnectionId = Context.ConnectionId,
            Username = username,
            RoomId = createdRoom.Id,
            IsHost = true
        };

        await _roomService.AddUserToRoomAsync(createdRoom.Id, user);
        await Groups.AddToGroupAsync(Context.ConnectionId, createdRoom.Id);



        await Clients.Caller.SendAsync("RoomCreated", createdRoom);
    }

    public async Task Play(TimeSpan position)
    {
        var user = await _roomService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user?.RoomId == null) 
        {
            await Clients.Caller.SendAsync("Error", "You are not in a room. Please join a room first.");
            return;
        }
        await _roomService.UpdateRoomStateAsync(user.RoomId, position, true);
        await Clients.Group(user.RoomId).SendAsync("Play", position);
    }

    public async Task Pause(TimeSpan position)
    {
        var user = await _roomService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user?.RoomId == null) 
        {
            await Clients.Caller.SendAsync("Error", "You are not in a room. Please join a room first.");
            return;
        }
        await _roomService.UpdateRoomStateAsync(user.RoomId, position, false);
        await Clients.Group(user.RoomId).SendAsync("Pause", position);
    }

    public async Task Seek(TimeSpan position)
    {
        var user = await _roomService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user?.RoomId == null) 
        {
            await Clients.Caller.SendAsync("Error", "You are not in a room. Please join a room first.");
            return;
        }
        await _roomService.UpdateRoomStateAsync(user.RoomId, position, null);
        await Clients.Group(user.RoomId).SendAsync("Seek", position);
    }

    public async Task ChangeMovie(string movieId)
    {
        var user = await _roomService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user?.RoomId == null) 
        {
            await Clients.Caller.SendAsync("Error", "You are not in a room. Please join a room first.");
            return;
        }
        
        if (!user.IsHost) 
        {
            await Clients.Caller.SendAsync("Error", "Only the room host can change movies.");
            return;
        }

        var movie = await _movieService.GetMovieAsync(movieId);
        if (movie == null)
        {
            await Clients.Caller.SendAsync("Error", $"Movie not found with ID: {movieId}");
            return;
        }


        await _roomService.SetCurrentMovieAsync(user.RoomId, movieId);
        await Clients.Group(user.RoomId).SendAsync("MovieChanged", movie);
    }

    public async Task SendMessage(string message)
    {
        var user = await _roomService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user?.RoomId == null) 
        {
            await Clients.Caller.SendAsync("Error", "You are not in a room. Please join a room first.");
            return;
        }

        await Clients.Group(user.RoomId).SendAsync("MessageReceived", new
        {
            Username = user.Username,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await _roomService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user?.RoomId != null)
        {
            await _roomService.RemoveUserFromRoomAsync(user.RoomId, Context.ConnectionId);
            await Clients.OthersInGroup(user.RoomId).SendAsync("UserLeft", user);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
} 