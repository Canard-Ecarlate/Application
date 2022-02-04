using DuckCity.Api.DTO.Room;
using DuckCity.Application.Services.Interfaces;
using DuckCity.Domain.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuckCity.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Room>> FindAllRooms() => new OkObjectResult(_roomService.FindAllRooms());

    [HttpPost]
    public ActionResult<Room> CreateAndJoinRoom(RoomCreationDto room)
    {
        Room roomCreated = _roomService.CreateAndJoinRoom(room.Name, room.HostId, room.HostName, room.IsPrivate, room.NbPlayers);
        return new OkObjectResult(roomCreated);
    }
        
    [HttpPost]
    public ActionResult<Room> JoinRoom(UserAndRoomDto userAndRoomDto)
    {
        Room roomJoined = _roomService.JoinRoom(userAndRoomDto.RoomId, userAndRoomDto.UserId, userAndRoomDto.UserName);
        return new OkObjectResult(roomJoined);
    }
}