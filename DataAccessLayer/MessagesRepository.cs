using Dapper;
using Imagekit.Constant;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public record MessageDto
    {
        public MessageDto(int id, int senderId, int? receiverId, string content, DateTime createdAt, int? roomId)
        {
            Id = id;
            SenderId = senderId;
            ReceiverId = receiverId;
            Content = content;
            CreatedAt = createdAt;
            RoomId = roomId;

        }

        [Key]
        public int Id { get; init; }

        [Required(ErrorMessage = "Sender ID is required")]
        public int SenderId { get; init; }

        //[Range(0, int.MaxValue, ErrorMessage = "Receiver ID should be greater or equal to 0")]
        public int? ReceiverId { get; init; }

        //[Range(0, int.MaxValue, ErrorMessage = "Room ID should be greater or equal to 0")]
        public int? RoomId { get; init; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
    public interface IMessagesRepository
    {
        Task<List<MessageDto>> GetAllMessagesAsync();
        Task<MessageDto?> GetMessageByIdAsync(int messageId);
        Task<List<MessageDto>> GetConversationAsync(int user1Id, int user2Id);
        Task<int> AddNewMessageAsync(MessageDto message);
        Task<bool> UpdateMessageAsync(MessageDto message);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<bool> IsMessageExistsByIdAsync(int messageId);
    }
    public class MessagesRepository : IMessagesRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<MessagesRepository> _logger;

        public MessagesRepository(NpgsqlDataSource dataSource, ILogger<MessagesRepository> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<List<MessageDto>> GetAllMessagesAsync()
        {
            try
            {
                await using var conn = await _dataSource.OpenConnectionAsync();
                var messages = await conn.QueryAsync<MessageDto>("SELECT * FROM Messages ORDER BY CreatedAt DESC");
                return messages.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all messages (GetAllMessagesAsync)");
                return new List<MessageDto>();
            }
        }

        public async Task<MessageDto?> GetMessageByIdAsync(int messageId)
        {
            try
            {
                const string sql = "SELECT * FROM Messages WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var message = await conn.QuerySingleOrDefaultAsync<MessageDto>(sql, new { Id = messageId });
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching message {messageId}, error in GetMessageByIdAsync");
                return null;
            }
        }

        public async Task<List<MessageDto>> GetConversationAsync(int user1Id, int user2Id)
        {
            if (user1Id == user2Id)
                return new List<MessageDto>();

            try
            {
                const string sql = @"
                    SELECT * FROM Messages 
                    WHERE (SenderId = @User1Id AND ReceiverId = @User2Id) 
                       OR (SenderId = @User2Id AND ReceiverId = @User1Id)
                    ORDER BY CreatedAt ASC";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var messages = await conn.QueryAsync<MessageDto>(sql, new { User1Id = user1Id, User2Id = user2Id });
                return messages.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching conversation between {user1Id} and {user2Id}, error in (GetConversationAsync)");
                return new List<MessageDto>();
            }
        }

        public async Task<int> AddNewMessageAsync(MessageDto message)
        {
            try
            {
                const string sql = @"
                    INSERT INTO Messages (SenderId, ReceiverId, RoomId, Content, CreatedAt)
                    VALUES (@SenderId, @ReceiverId,@RoomId, @Content, @CreatedAt)
                    RETURNING Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var InsertedId = await conn.ExecuteScalarAsync<int>(sql, message);
                return InsertedId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding message from {message.SenderId} to {message.ReceiverId}, error in AddNewMessageAsync");
                return -1;
            }
        }

        public async Task<bool> UpdateMessageAsync(MessageDto message)
        {
            try
            {
                const string sql = @"
                    UPDATE Messages 
                    SET Content = @Content 
                    WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, message);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating message {message.Id}, error in UpdateMessageAsync");
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            try
            {
                const string sql = "DELETE FROM Messages WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { Id = messageId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {messageId}, error in DeleteMessageAsync");
                return false;
            }
        }

        public async Task<bool> IsMessageExistsByIdAsync(int messageId)
        {
            try
            {
                const string sql = "SELECT 1 FROM Messages WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                return await conn.QueryFirstOrDefaultAsync<int>(sql, new { Id = messageId }) == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking message {messageId} existence, error in IsMessageExistsByIdAsync");
                return false;
            }
        }

        public async Task<bool> IsUserExistsAsync(int? userId)
        {
            try
            {
                const string sql = "SELECT 1 FROM Users WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var isExists = await conn.QueryFirstOrDefaultAsync<int>(sql, new { Id = userId });
                return isExists > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in IsUserExistsAsync for userId: {userId}");
                return false;
            }
        }
    }
}