using Microsoft.AspNetCore.SignalR.Client;
using WatchSyncMovie.Client.Models;
using System.Text.Json;

namespace WatchSyncMovie.Client.Services;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    public SignalRService(string hubUrl = "https://localhost:7083/movieSyncHub")
    {
        _hubUrl = hubUrl;
    }

    public event Action<Room>? RoomJoined;
    public event Action<Room>? RoomCreated;
    public event Action<User>? UserJoined;
    public event Action<User>? UserLeft;
    public event Action<Movie>? MovieChanged;
    public event Action<TimeSpan>? PlayReceived;
    public event Action<TimeSpan>? PauseReceived;
    public event Action<TimeSpan>? SeekReceived;
    public event Action<object>? SyncStateReceived;
    public event Action<string>? ErrorReceived;
    public event Action<object>? MessageReceived;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public async Task StartAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Register event handlers
        _hubConnection.On<Room>("RoomJoined", room => RoomJoined?.Invoke(room));
        _hubConnection.On<Room>("RoomCreated", room => RoomCreated?.Invoke(room));
        _hubConnection.On<User>("UserJoined", user => UserJoined?.Invoke(user));
        _hubConnection.On<User>("UserLeft", user => UserLeft?.Invoke(user));
        _hubConnection.On<Movie>("MovieChanged", movie => MovieChanged?.Invoke(movie));
        _hubConnection.On<TimeSpan>("Play", position => PlayReceived?.Invoke(position));
        _hubConnection.On<TimeSpan>("Pause", position => PauseReceived?.Invoke(position));
        _hubConnection.On<TimeSpan>("Seek", position => SeekReceived?.Invoke(position));
        _hubConnection.On<object>("SyncState", state => SyncStateReceived?.Invoke(state));
        _hubConnection.On<string>("Error", error => ErrorReceived?.Invoke(error));
        _hubConnection.On<object>("MessageReceived", message => MessageReceived?.Invoke(message));

        await _hubConnection.StartAsync();
    }

    public async Task JoinRoomAsync(string roomId, string username, string? password = null)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("JoinRoom", roomId, username, password);
        }
    }

    public async Task CreateRoomAsync(string roomName, string username, string? password = null)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("CreateRoom", roomName, username, password);
        }
    }

    public async Task PlayAsync(TimeSpan position)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("Play", position);
        }
    }

    public async Task PauseAsync(TimeSpan position)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("Pause", position);
        }
    }

    public async Task SeekAsync(TimeSpan position)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("Seek", position);
        }
    }

    public async Task ChangeMovieAsync(string movieId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("ChangeMovie", movieId);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMessage", message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
} 