﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BusinessLayer;
using Microsoft.AspNetCore.Authorization;
using DataAccessLayer.Productions;
using DataAccessLayer;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomsService _roomsService;
        private readonly IUsersService _usersService;

        public RoomsController(IRoomsService roomService, IUsersService usersService)
        {
            _roomsService = roomService;
            _usersService = usersService;
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RoomDto>>> GetAllRooms()
        {
            var rooms = await _roomsService.GetAllRoomsAsync();
            return rooms.Count == 0
                ? NotFound("No rooms found")
                : Ok(new { Message = "Rooms retrieved successfully", Data = rooms });
        }

        [HttpGet("GetRoomsPaginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RoomDto>>> GetRoomsPaginated(int pageNumber = 1, int pageSize = 10)
        {
            var rooms = await _roomsService.GetRoomsPaginatedAsync(pageNumber, pageSize);
            return rooms.Count == 0
                ? NotFound("No rooms found")
                : Ok(new { Message = "Rooms retrieved successfully", Data = rooms });
        }

        [HttpGet("GetById/{roomId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomDto>> GetRoomById(int roomId)
        {
            if (roomId < 1)
                return BadRequest("Invalid room ID");

            var room = await _roomsService.GetRoomByIdAsync(roomId);
            return room == null
                ? NotFound($"Room {roomId} not found")
                : Ok(new { Message = "Room retrieved successfully", Data = room });
        }
        [Authorize]
        [HttpGet("GetUsersDetailsDataInRoomByRoomId/{roomId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<UserDto>>> GetUsersInRoomByRoomIdAsync(int roomId)
        {
            if (!await _roomsService.IsRoomExistsByIdAsync(roomId))
                return NotFound($"Room {roomId} not found");

            var users = await _roomsService.GetUsersDetailsDataInRoomByRoomIdAsync(roomId);
            return Ok(new { Message = "Room users retrieved", Data = users });
        }

        [HttpGet("GetMessagesInRoomByRoomId/{roomId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<MessageDto>>> GetMessagesInRoomByRoomIdAsync(int roomId)
        {
            if (!await _roomsService.IsRoomExistsByIdAsync(roomId))
                return NotFound($"Room {roomId} not found");

            var users = await _roomsService.GetMessagesInRoomByRoomIdAsync(roomId);
            return Ok(new { Message = "Room Messages retrieved", Data = users });
        }


        [HttpGet("GetCurrentRoomsForCurrentUser/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RoomDto>>> GetCurrentRoomsForCurrentUserAsync(int pageNumber, int pageSize, int userId)
        {
            if (!await _usersService.IsUserExistsByUserIDAsync(userId))
                return NotFound($"User with {userId} not found");

            var users = await _roomsService.GetCurrentRoomsForCurrentUserPaginatedAsync(pageNumber, pageSize, userId);
            return Ok(new { Message = "Room Messages retrieved", Data = users });
        }


        [Authorize]
        [HttpPost("CreateRoom")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] RoomDto room)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newRoomId = await _roomsService.AddNewRoomAsync(room);
            if (newRoomId < 0)
                return BadRequest("Failed to create room");

            room = new RoomDto(newRoomId, room.Name, room.Description, DateTime.UtcNow, room.CreatorId);
            return CreatedAtAction(nameof(GetRoomById),
                new { roomId = room.Id },
                new { Message = "Room created successfully", Data = room });
        }

        [Authorize]
        [HttpPut("UpdateRoom/{roomId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoomDto>> UpdateRoom(int roomId, [FromBody] RoomDto room)
        {
            if (roomId < 1 || !ModelState.IsValid)
                return BadRequest("Invalid request parameters");

            if (!await _roomsService.IsRoomExistsByIdAsync(roomId))
                return NotFound($"Room {roomId} not found");

            room = room with { Id = roomId };
            if (!await _roomsService.UpdateRoomAsync(room))
                return BadRequest("Update failed");

            return Ok(new { Message = "Room updated successfully", Data = room });
        }

        [Authorize]
        [HttpDelete("DeleteRoom/{roomId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteRoom(int roomId)
        {
            if (roomId < 1)
                return BadRequest("Invalid room ID");

            if (!await _roomsService.IsRoomExistsByIdAsync(roomId))
                return NotFound($"Room {roomId} not found");

            if (!await _roomsService.DeleteRoomAsync(roomId))
                return StatusCode(500, "Delete operation failed");

            return Ok(new { Message = $"Room {roomId} deleted successfully" });
        }

        [HttpGet("GetRoomUsers/{roomId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RoomUserDto>>> GetRoomUsers(int roomId)
        {
            if (!await _roomsService.IsRoomExistsByIdAsync(roomId))
                return NotFound($"Room {roomId} not found");

            var users = await _roomsService.GetRoomUsersAsync(roomId);
            return Ok(new { Message = "Room users retrieved", Data = users });
        }

        [Authorize]
        [HttpPost("AddUserToRoom/{roomId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddUserToRoom(int roomId, int userId)
        {
            if (!await _roomsService.IsRoomExistsByIdAsync(roomId))
                return NotFound($"Room {roomId} not found");

            if (!await _roomsService.AddUserToRoomAsync(roomId, userId))
                return BadRequest("Failed to add user to room");

            return Ok(new { Message = $"User {userId} added to room {roomId}" });
        }

        [Authorize]
        [HttpDelete("DeleteUserFromRoom/{roomId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUserFromRoom(int roomId, int userId)
        {
            if (roomId < 1)
                return BadRequest("Invalid room ID");
            if (userId < 0)
                return BadRequest("Invalid user ID");

            if (!await _roomsService.IsRoomExistsByIdAsync(roomId))
                return NotFound($"Room {roomId} not found");

            if (!await _roomsService.DeleteUserFromRoomAsync(roomId, userId))
                return StatusCode(500, "Delete operation failed");

            return Ok(new { Message = $"User {userId} from Room {roomId} deleted successfully" });
        }


    }
}