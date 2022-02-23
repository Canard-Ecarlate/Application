﻿using DuckCity.Domain.Exceptions;
using DuckCity.Domain.Rooms;
using DuckCity.Domain.Users;
using DuckCity.Infrastructure.RoomRepository;

namespace DuckCity.Application.RoomService;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public void CreateRoom(Room newRoom)
    {
        _roomRepository.Create(newRoom);
    }

    public Room JoinRoom(string connectionId, string userId, string userName, string roomCode)
    {
        Room room = _roomRepository.FindByCode(roomCode)!;
        room.Players.Add(new Player(connectionId, userId, userName));
        _roomRepository.Update(room);
        return room;
    }

    public Room ReconnectRoom(string connectionId, string userId)
    {
        Room? room = _roomRepository.FindByUserId(userId);
        Player? player = room?.Players.SingleOrDefault(p => p.Id == userId);
        if (room == null || player == null)
        {
            throw new PlayerNotFoundException(userId + " not found");
        }
        player.ConnectionId = connectionId;
        _roomRepository.Update(room);

        return room;
    }
    public Room? LeaveRoom(string roomCode, string connectionId)
    {
        Room room = _roomRepository.FindByCode(roomCode)!;
        Player player = room.Players.Single(p => p.ConnectionId == connectionId);
        room.Players.Remove(player);
        if (room.Players is not {Count: 0})
        {
            _roomRepository.Update(room);
            return room;
        }

        _roomRepository.Delete(room);
        return null;
    }

    /**
     * Update player info (connectionId) when he's disconnected
     */
    public Room? DisconnectFromRoom(string connectionId)
    {
        Room? room = _roomRepository.FindByConnectionId(connectionId);
        if (room == null)
        {
            return room;
        }

        Player player = room.Players.Single(p => p.ConnectionId == connectionId);
        player.ConnectionId = null;
        _roomRepository.Update(room);
        return room;
    }

    public Room SetPlayerReady(string roomCode, string connectionId)
    {
        Room room = _roomRepository.FindByCode(roomCode)!;
        Player player = room.Players.Single(p => p.ConnectionId == connectionId);
        player.Ready = !player.Ready;
        _roomRepository.Update(room);
        return room;
    }

    public bool UserIsInRoom(string userId)
    {
        return _roomRepository.FindByUserId(userId) != null;
    }
}