using AutoMapper;
using DuckCity.Application.RoomPreviewService;
using DuckCity.Application.RoomService;
using DuckCity.Domain.Rooms;
using DuckCity.GameApi.Dto;
using Microsoft.AspNetCore.SignalR;

namespace DuckCity.GameApi.Hub;

public class DuckCityHub : Hub<IDuckCityClient>
{
    private readonly IRoomService _roomService;
    private readonly IRoomPreviewService _roomPreviewService;
    private readonly IMapper _mapper;

    // Constructor
    public DuckCityHub(IRoomService roomService, IMapper mapper, IRoomPreviewService roomPreviewService)
    {
        _roomService = roomService;
        _mapper = mapper;
        _roomPreviewService = roomPreviewService;
    }

    /**
     * Methods 
     */
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Room? room = _roomService.DisconnectFromRoom(Context.ConnectionId);
        if (room != null)
        {
            // Send
            await Clients.Group(room.Id).PushPlayers(_mapper.Map<IEnumerable<PlayerInWaitingRoomDto>>(room.Players));
        }

        await base.OnDisconnectedAsync(exception);
    }

    [HubMethodName("CreateRoom")]
    public async Task CreateRoomAsync(RoomCreationDto roomDto)
    {
        Room newRoom = new(roomDto.RoomName, roomDto.HostId, roomDto.HostName, roomDto.ContainerId, roomDto.IsPrivate,
            roomDto.NbPlayers, Context.ConnectionId, _roomPreviewService.GenerateCode());
        
        // Create Room
        _roomService.CreateRoom(newRoom);
        
        // Create RoomPreview from Room
        _roomPreviewService.CreateRoomPreview(new RoomPreview(newRoom));
        
        // Join SignalR
        await Groups.AddToGroupAsync(Context.ConnectionId, newRoom.Id);

        // Send
        await Clients.Group(newRoom.Id).PushPlayers(_mapper.Map<IEnumerable<PlayerInWaitingRoomDto>>(newRoom.Players));
    }


    [HubMethodName("JoinRoom")]
    public async Task JoinRoomAsync(string roomCode, string userId, string userName)
    {
        // Join Room
        Room room = _roomService.JoinRoom(Context.ConnectionId, userId, userName, roomCode);
       
        // Update RoomPreview from Room
        _roomPreviewService.UpdateRoomPreview(new RoomPreview(room));
        
        // Join SignalR
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        // Send
        await Clients.Group(roomCode).PushPlayers(_mapper.Map<IEnumerable<PlayerInWaitingRoomDto>>(room.Players));
    }

    [HubMethodName("LeaveRoom")]
    public async Task LeaveRoomAsync(string roomId, string userId)
    {
        // Leave Room
        Room? room = _roomService.LeaveRoom(roomId, Context.ConnectionId);

        // Leave SignalR
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        if (room != null)
        {
            // Update RoomPreview
            _roomPreviewService.UpdateRoomPreview(new RoomPreview(room));
            
            // Send
            await Clients.Group(roomId).PushPlayers(_mapper.Map<IEnumerable<PlayerInWaitingRoomDto>>(room.Players));
        }
        else
        {
            // Delete RoomPreview
            _roomPreviewService.DeleteRoomPreview(roomId);
        }
    }

    [HubMethodName("PlayerReady")]
    public async Task PlayerReadyAsync(string roomId)
    {
        Room room = _roomService.SetPlayerReady(roomId, Context.ConnectionId);

        // Send
        await Clients.Group(roomId).PushPlayers(_mapper.Map<IEnumerable<PlayerInWaitingRoomDto>>(room.Players));
    }
}