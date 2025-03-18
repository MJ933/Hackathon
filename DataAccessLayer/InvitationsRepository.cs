using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface IInvitationsRepository
    {
        Task<List<InvitationDto>> GetAllInvitationsAsync();
        Task<InvitationDto?> GetInvitationByIdAsync(int invitationId);
        Task<int> AddNewInvitationAsync(InvitationDto invitation);
        Task<bool> UpdateInvitationAsync(InvitationDto invitation);
        Task<bool> DeleteInvitationAsync(int invitationId);
        Task<bool> IsInvitationExistsByIdAsync(int invitationId);
    }

    public class InvitationsRepository : IInvitationsRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<InvitationsRepository> _logger;

        public InvitationsRepository(NpgsqlDataSource dataSource, ILogger<InvitationsRepository> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<List<InvitationDto>> GetAllInvitationsAsync()
        {
            try
            {
                await using var conn = await _dataSource.OpenConnectionAsync();
                var invitations = await conn.QueryAsync<InvitationDto>(
                    "SELECT * FROM Invitations ORDER BY CreatedAt DESC");
                return invitations.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all invitations");
                return new List<InvitationDto>();
            }
        }

        public async Task<InvitationDto?> GetInvitationByIdAsync(int invitationId)
        {
            try
            {
                const string sql = "SELECT * FROM Invitations WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                return await conn.QuerySingleOrDefaultAsync<InvitationDto>(sql, new { Id = invitationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching invitation {invitationId}");
                return null;
            }
        }

        public async Task<int> AddNewInvitationAsync(InvitationDto invitation)
        {
            try
            {
                const string sql = @"
                    INSERT INTO Invitations (InviterId, InviteeId, RoomId, Status)
                    VALUES (@InviterId, @InviteeId, @RoomId, @Status)
                    RETURNING Id";

                await using var conn = await _dataSource.OpenConnectionAsync();
                return await conn.ExecuteScalarAsync<int>(sql, invitation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding invitation from {invitation.InviterId} to {invitation.InviteeId}");
                return -1;
            }
        }

        public async Task<bool> UpdateInvitationAsync(InvitationDto invitation)
        {
            try
            {
                const string sql = @"
                    UPDATE Invitations
                    SET Status = @Status,
                        UpdatedAt = CURRENT_TIMESTAMP
                    WHERE Id = @Id";

                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { Status = invitation.Status, Id = invitation.Id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating invitation {invitation.Id}");
                return false;
            }
        }

        public async Task<bool> DeleteInvitationAsync(int invitationId)
        {
            try
            {
                const string sql = "DELETE FROM Invitations WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { Id = invitationId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting invitation {invitationId}");
                return false;
            }
        }

        public async Task<bool> IsInvitationExistsByIdAsync(int invitationId)
        {
            try
            {
                const string sql = "SELECT 1 FROM Invitations WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var result = await conn.QueryFirstOrDefaultAsync<int>(sql, new { Id = invitationId });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking invitation {invitationId} existence");
                return false;
            }
        }
    }

    public record InvitationDto
    {
        public InvitationDto(int id, int inviterId, int inviteeId, int roomId, string status, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            InviterId = inviterId;
            InviteeId = inviteeId;
            RoomId = roomId;
            Status = status;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        [Key]
        public int Id { get; init; }

        [Required(ErrorMessage = "Inviter ID is required")]
        public int InviterId { get; init; }

        [Required(ErrorMessage = "Invitee ID is required")]
        public int InviteeId { get; init; }

        [Required(ErrorMessage = "Room ID is required")]
        public int RoomId { get; init; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; init; } = "Pending";

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    }
}