using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public record ConnectionDto
    {
        public ConnectionDto(int userId, int connectedUserId, string status, DateTime createdAt)
        {
            UserId = userId;
            ConnectedUserId = connectedUserId;
            Status = status;
            CreatedAt = createdAt;
        }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; init; }

        [Required(ErrorMessage = "Connected User ID is required")]
        public int ConnectedUserId { get; init; }

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Pending|Accepted|Canceled)$", ErrorMessage = "Invalid connection status")]
        public string Status { get; init; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
    //public record ConnectionDetailDto : ConnectionDto
    //{
    //    public ConnectionDetailDto(ConnectionDto connectionDto)
    //        : base(connectionDto.UserId, connectionDto.ConnectedUserId, connectionDto.Status, connectionDto.CreatedAt)
    //    {
    //        _userRepo = _userRepo ?? throw new ArgumentNullException(nameof(_userRepo));

    //        UserInfo = _userRepo.GetUserByUserIDAsync(connectionDto.UserId).Result;
    //        ConnectedUserInfo = _userRepo.GetUserByUserIDAsync(connectionDto.ConnectedUserId).Result;
    //    }
    //    private readonly IUsersRepository _userRepo;
    //    public UserDto? UserInfo { get; set; }
    //    public UserDto? ConnectedUserInfo { get; set; }
    //}

    public interface IConnectionsRepository
    {
        Task<List<ConnectionDto>> GetAllConnectionsAsync();
        Task<ConnectionDto?> GetConnectionByIdsAsync(int userId, int connectedUserId);
        Task<bool> AddConnectionAsync(ConnectionDto connection);
        Task<bool> UpdateConnectionStatusAsync(ConnectionDto connection);
        Task<bool> DeleteConnectionAsync(int userId, int connectedUserId);
        Task<bool> IsConnectionExistsAsync(int userId, int connectedUserId);
    }

    public class ConnectionsRepository : IConnectionsRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<ConnectionsRepository> _logger;

        public ConnectionsRepository(NpgsqlDataSource dataSource, ILogger<ConnectionsRepository> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<List<ConnectionDto>> GetAllConnectionsAsync()
        {
            try
            {
                await using var conn = await _dataSource.OpenConnectionAsync();
                var connections = await conn.QueryAsync<ConnectionDto>(
                    "SELECT * FROM Connections ORDER BY CreatedAt DESC"
                );
                return connections.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all connections, Error in GetAllConnectionsAsync()");
                return new List<ConnectionDto>();
            }
        }

        public async Task<ConnectionDto?> GetConnectionByIdsAsync(int userId, int connectedUserId)
        {
            try
            {
                const string sql = @"
                    SELECT * FROM Connections 
                    WHERE (UserId = @UserId AND ConnectedUserId = @ConnectedUserId)
                    OR (UserId = @ConnectedUserId AND ConnectedUserId = @UserId)";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var connectionDto = await conn.QuerySingleOrDefaultAsync<ConnectionDto>(sql, new { UserId = userId, ConnectedUserId = connectedUserId });
                return connectionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching connection between {userId} and {connectedUserId}, Error in GetConnectionByIdsAsync");
                return null;
            }
        }
        public async Task<bool> AddConnectionAsync(ConnectionDto connection)
        {
            try
            {
                const string sql = @"
                    INSERT INTO Connections (UserId, ConnectedUserId, Status, CreatedAt)
                    VALUES (@UserId, @ConnectedUserId, @Status, @CreatedAt)";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, connection);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding connection between {connection.UserId} and {connection.ConnectedUserId}, Error in AddConnectionAsync");
                return false;
            }
        }

        public async Task<bool> UpdateConnectionStatusAsync(ConnectionDto connection)
        {
            try
            {
                const string sql = @"
                    UPDATE Connections 
                    SET Status = @NewStatus 
                    WHERE (UserId = @UserId AND ConnectedUserId = @ConnectedUserId)
                    OR (UserId = @ConnectedUserId AND ConnectedUserId = @UserId)";
                var parameters = new
                {
                    UserId = connection.UserId,
                    ConnectedUserId = connection.ConnectedUserId,
                    NewStatus = connection.Status
                };

                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating connection status between {connection.UserId} and {connection.ConnectedUserId}, Error in UpdateConnectionStatusAsync");
                return false;
            }
        }

        public async Task<bool> DeleteConnectionAsync(int userId, int connectedUserId)
        {
            try
            {
                const string sql = @"
                    DELETE FROM Connections 
                    WHERE (UserId = @UserId AND ConnectedUserId = @ConnectedUserId)
                    OR (UserId = @ConnectedUserId AND ConnectedUserId = @UserId)";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { UserId = userId, ConnectedUserId = connectedUserId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting connection between {userId} and {connectedUserId}, Error in DeleteConnectionAsync");
                return false;
            }
        }

        public async Task<bool> IsConnectionExistsAsync(int userId, int connectedUserId)
        {
            try
            {
                const string sql = @"
                    SELECT 1 FROM Connections 
                    WHERE (UserId = @UserId AND ConnectedUserId = @ConnectedUserId)
                    OR (UserId = @ConnectedUserId AND ConnectedUserId = @UserId)";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var result = await conn.QueryFirstOrDefaultAsync<int>(sql, new { UserId = userId, ConnectedUserId = connectedUserId });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking connection existence between {userId} and {connectedUserId}, Error in IsConnectionExistsAsync");
                return false;
            }
        }
    }
}