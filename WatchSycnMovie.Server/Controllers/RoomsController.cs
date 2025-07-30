using Microsoft.AspNetCore.Mvc;
using WatchSyncMovie.Server.Models;
using WatchSyncMovie.Server.Services;

namespace WatchSyncMovie.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Room>>> GetActiveRooms()
    {
        var rooms = await _roomService.GetActiveRoomsAsync();
        // Remove sensitive information like passwords from the response
        var publicRooms = rooms.Select(r => new
        {
            r.Id,
            r.Name,
            HasPassword = !string.IsNullOrEmpty(r.Password),
            UserCount = r.Users.Count,
            r.CreatedAt,
            CurrentMovie = r.CurrentMovieId,
            r.IsPlaying
        }).ToList();
        
        return Ok(publicRooms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetRoom(string id)
    {
        var room = await _roomService.GetRoomAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        // Remove password from response
        var publicRoom = new
        {
            room.Id,
            room.Name,
            HasPassword = !string.IsNullOrEmpty(room.Password),
            room.CurrentMovieId,
            room.CurrentPosition,
            room.IsPlaying,
            room.Users,
            room.CreatedAt
        };

        return Ok(publicRoom);
    }
} 