using Dapper;
using DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public record RoomDto
    {
        public RoomDto(int id, string name, string description, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Description = description;
            CreatedAt = createdAt;
        }

        [Key]
        public int Id { get; init; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTime CreatedAt { get; init; }
    }
    public record RoomUserDto
    {
        public int UserId { get; init; }
        public DateTime JoinedAt { get; init; }
    }

    public interface IRoomsRepository
    {
        Task<List<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto?> GetRoomByIdAsync(int roomId);
        Task<int> AddNewRoomAsync(RoomDto room);
        Task<bool> UpdateRoomAsync(RoomDto room);
        Task<bool> DeleteRoomAsync(int roomId);
        Task<bool> IsRoomExistsByIdAsync(int roomId);
        Task<List<RoomUserDto>> GetRoomUsersAsync(int roomId);
        Task<bool> AddUserToRoomAsync(int roomId, int userId);
        Task<bool> DeleteUserFromRoomAsync(int roomId, int userId);

        Task<List<UserDto>> GetUsersDetailsDataInRoomByRoomIdAsync(int roomId);
        Task<List<MessageDto>> GetMessagesInRoomByRoomIdAsync(int roomId);
    }

    public class RoomsRepository : IRoomsRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<RoomsRepository> _logger;

        public RoomsRepository(NpgsqlDataSource dataSource, ILogger<RoomsRepository> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<List<RoomDto>> GetAllRoomsAsync()
        {
            try
            {
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rooms = await conn.QueryAsync<RoomDto>("SELECT * FROM Rooms order by Id desc");
                return rooms.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all rooms");
                return new List<RoomDto>();
            }
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int roomId)
        {
            try
            {
                const string sql = "SELECT * FROM Rooms WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var room = await conn.QuerySingleOrDefaultAsync<RoomDto>(sql, new { Id = roomId });
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching room {roomId}");
                return null;
            }
        }

        public async Task<int> AddNewRoomAsync(RoomDto room)
        {
            try
            {
                const string sql = @"
                    INSERT INTO Rooms (Name, Description)
                    VALUES (@Name, @Description)
                    RETURNING Id";

                await using var conn = await _dataSource.OpenConnectionAsync();
                var LastInsertedRoomId = await conn.ExecuteScalarAsync<int>(sql, room);
                return LastInsertedRoomId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating room {room.Name}");
                return -1;
            }
        }

        public async Task<bool> UpdateRoomAsync(RoomDto room)
        {
            try
            {
                const string sql = @"
                    UPDATE Rooms
                    SET Name = @Name, Description = @Description
                    WHERE Id = @Id";

                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, room);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating room {room.Id}");
                return false;
            }
        }

        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            try
            {
                const string sql = "DELETE FROM Rooms WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { Id = roomId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting room {roomId}");
                return false;
            }
        }

        public async Task<bool> IsRoomExistsByIdAsync(int roomId)
        {
            try
            {
                const string sql = "SELECT 1 FROM Rooms WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.QueryFirstOrDefaultAsync<int>(sql, new { Id = roomId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking room {roomId} existence");
                return false;
            }
        }

        public async Task<List<RoomUserDto>> GetRoomUsersAsync(int roomId)
        {
            try
            {
                const string sql = @"
                    SELECT u.Id AS UserId, ru.JoinedAt
                    FROM RoomUsers ru
                    JOIN Users u ON ru.UserId = u.Id
                    WHERE ru.RoomId = @RoomId";

                await using var conn = await _dataSource.OpenConnectionAsync();
                var roomUsers = await conn.QueryAsync<RoomUserDto>(sql, new { RoomId = roomId });
                return roomUsers.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching users for room {roomId}");
                return new List<RoomUserDto>();
            }
        }

        public async Task<bool> AddUserToRoomAsync(int roomId, int userId)
        {
            try
            {
                const string sql = @"
                    INSERT INTO RoomUsers (RoomId, UserId)
                    VALUES (@RoomId, @UserId)";

                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { RoomId = roomId, UserId = userId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding user {userId} to room {roomId}");
                return false;
            }
        }

        public async Task<bool> DeleteUserFromRoomAsync(int roomId, int userId)
        {
            try
            {
                const string sql = @"DELETE FROM RoomUsers WHERE UserId = @UserId and RoomId = @RoomId";

                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { RoomId = roomId, UserId = userId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error Delete user {userId} From room {roomId}");
                return false;
            }
        }

        public async Task<List<UserDto>> GetUsersDetailsDataInRoomByRoomIdAsync(int roomId)
        {
            try
            {
                const string sql = @"select u.* from users u 
                                inner join roomUsers rm on u.id = rm.UserId and rm.roomId = @roomId;";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var usersLists = await conn.QueryAsync<UserDto>(sql, new { roomId = roomId });
                return usersLists.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error in GetUsersInRoomByRoomId for roomId: {roomId}");
                return new List<UserDto>();
            }
        }
        public async Task<List<MessageDto>> GetMessagesInRoomByRoomIdAsync(int roomId)
        {
            try
            {
                const string sql = @"select * from messages where messages.roomid  = @roomid ";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var messagesList = await conn.QueryAsync<MessageDto>(sql, new { roomid = roomId });
                return messagesList.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"error in GetUsersInRoomByRoomId for roomId: {roomId}");
                return new List<MessageDto>();
            }
        }
    }
}




