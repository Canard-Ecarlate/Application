﻿using System.Collections.Generic;
using DuckCity.Application.Services;
using DuckCity.Domain.Exceptions;
using DuckCity.Domain.Rooms;
using DuckCity.Infrastructure.Repositories.Interfaces;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace DuckCity.Application.Tests.Services;

public class RoomServiceTests
{
    private readonly RoomService _roomService;
    private readonly Mock<IUserRepository> _mockUserRep = new();
    private readonly Mock<IRoomRepository> _mockRoomRep = new();

    public RoomServiceTests()
    {
        _roomService = new RoomService(_mockUserRep.Object, _mockRoomRep.Object);
    }

    [Fact]
    public void FindAllRoomsTest()
    {
        _mockRoomRep.Setup(mock => mock.FindAllRooms()).Returns(new List<Room>());

        IEnumerable<Room> result = _roomService.FindAllRooms();
        Assert.Empty(result);
        _mockRoomRep.Verify(mock => mock.FindAllRooms(), Times.Once);
    }

    [Theory]
    [InlineData("something not ObjectId")]
    [InlineData(Examples.ObjectId)]
    public void FindRoomTest(string roomId)
    {
        _mockRoomRep.Setup(mock => mock.FindById(roomId)).Returns(new Room());

        try
        {
            Room? result = _roomService.FindRoom(roomId);
            Assert.NotNull(result);
            _mockRoomRep.Verify(mock => mock.FindById(roomId), Times.Once);
        }
        catch (IdNotValidException e)
        {
            if (!ObjectId.TryParse(roomId, out _))
            {
                // error because an id is not valid => OK
            }
            else
            {
                throw;
            }
        }
    }

    [Theory]
    [InlineData(Examples.String, Examples.ObjectId, Examples.String, Examples.True, Examples.Five, 1)]
    [InlineData(Examples.String, Examples.String, Examples.String, Examples.True, Examples.Five, 1)]
    [InlineData(Examples.String, Examples.ObjectId, Examples.String, Examples.True, Examples.Five, 0)]
    [InlineData("", Examples.ObjectId, Examples.String, Examples.True, Examples.Five, 1)]
    public void AddRoomsTest(string roomName, string hostId, string hostName, bool isPrivate, int nbPlayers, int countUser)
    {
        _mockUserRep.Setup(mock => mock.CountUserById(hostId)).Returns(countUser);
        
        try
        {
            Room roomResult = _roomService.AddRooms(roomName, hostId, hostName, isPrivate, nbPlayers);
            Assert.NotNull(roomResult);
            Assert.NotNull(roomResult.RoomConfiguration);
            Assert.Equal(roomName, roomResult.Name);
            Assert.Equal(hostId, roomResult.HostId);
            Assert.Equal(hostName, roomResult.HostName);
            Assert.Equal(isPrivate, roomResult.RoomConfiguration?.IsPrivate);
            Assert.Equal(nbPlayers, roomResult.RoomConfiguration?.NbPlayers);
            _mockRoomRep.Verify(mock => mock.Create(It.IsAny<Room>()), Times.Once);
        }
        catch (IdNotValidException e)
        {
            if (!ObjectId.TryParse(hostId, out _))
            {
                // error because an id is not valid => OK
            }
            else
            {
                throw;
            }        }
        catch (RoomNameNullException e)
        {
            if (string.IsNullOrEmpty(roomName))
            {
                // error because an id is not valid => OK
            }
            else
            {
                throw;
            }        }
        catch (HostIdNoExistException e)
        {
            if (_mockUserRep.Object.CountUserById(hostId) == 0)
            {
                // error because an id is not valid => OK
            }
            else
            {
                throw;
            }        }
    }
}