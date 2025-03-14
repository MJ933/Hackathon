// DataAccessLayer/SkillsRepository.cs
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface ISkillsRepository
    {
        Task<List<SkillDto>> GetAllSkillsAsync();
        Task<SkillDto?> GetSkillByIdAsync(int skillId);
        Task<int> AddSkillAsync(SkillDto skill);
        Task<bool> UpdateSkillAsync(SkillDto skill);
        Task<bool> DeleteSkillAsync(int skillId);
        Task<bool> IsSkillExistsAsync(int skillId);
        Task<bool> AddUserSkillAsync(int userId, int skillId);
        Task<bool> RemoveUserSkillAsync(int userId, int skillId);
        Task<List<SkillDto>> GetSkillsByUserIdAsync(int userId);
        Task<List<UserDto>> GetUsersBySkillIdAsync(int skillId);
    }

    public class SkillsRepository : ISkillsRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<SkillsRepository> _logger;

        public SkillsRepository(NpgsqlDataSource dataSource, ILogger<SkillsRepository> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<SkillDto>> GetAllSkillsAsync()
        {
            try
            {
                await using var conn = await _dataSource.OpenConnectionAsync();
                return (await conn.QueryAsync<SkillDto>("SELECT * FROM Skills")).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllSkillsAsync");
                return new List<SkillDto>();
            }
        }

        public async Task<SkillDto?> GetSkillByIdAsync(int skillId)
        {
            try
            {
                await using var conn = await _dataSource.OpenConnectionAsync();
                return await conn.QuerySingleOrDefaultAsync<SkillDto>(
                    "SELECT * FROM Skills WHERE Id = @Id", new { Id = skillId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetSkillByIdAsync: {skillId}");
                return null;
            }
        }

        public async Task<int> AddSkillAsync(SkillDto skill)
        {
            try
            {
                const string sql = "INSERT INTO Skills (Name) VALUES (@Name) RETURNING Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var insertedSkillID = await conn.ExecuteScalarAsync<int>(sql, new { Name = skill.Name });
                return insertedSkillID ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding skill: {skill.Name}");
                return -1;
            }
        }

        public async Task<bool> UpdateSkillAsync(SkillDto skill)
        {
            try
            {
                const string sql = "UPDATE Skills SET Name = @Name WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var result = await conn.ExecuteAsync(sql, new { Id = skill.Id, Name = skill.Name });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating for skillId: {skill.Id}");
                return false;
            }
        }

        public async Task<bool> DeleteSkillAsync(int skillId)
        {
            try
            {
                const string sql = "DELETE FROM Skills WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                return await conn.ExecuteAsync(sql, new { Id = skillId }) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting skill: {skillId}");
                return false;
            }
        }

        public async Task<bool> IsSkillExistsAsync(int skillId)
        {
            try
            {
                const string sql = "SELECT 1 FROM Skills WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var result = await conn.ExecuteScalarAsync<int>(sql, new { Id = skillId });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking skill existence: {skillId}");
                return false;
            }
        }

        public async Task<bool> AddUserSkillAsync(int userId, int skillId)
        {
            try
            {
                const string sql = @"
                    INSERT INTO UserSkills (UserId, SkillId) 
                    VALUES (@UserId, @SkillId)
                    ON CONFLICT (UserId, SkillId) DO NOTHING";

                await using var conn = await _dataSource.OpenConnectionAsync();
                return await conn.ExecuteAsync(sql, new { UserId = userId, SkillId = skillId }) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding user skill: User {userId}, Skill {skillId}");
                return false;
            }
        }

        public async Task<bool> RemoveUserSkillAsync(int userId, int skillId)
        {
            try
            {
                const string sql = "DELETE FROM UserSkills WHERE UserId = @UserId AND SkillId = @SkillId";
                await using var conn = await _dataSource.OpenConnectionAsync();
                return await conn.ExecuteAsync(sql, new { UserId = userId, SkillId = skillId }) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing user skill: User {userId}, Skill {skillId}");
                return false;
            }
        }

        public async Task<List<SkillDto>> GetSkillsByUserIdAsync(int userId)
        {
            try
            {
                const string sql = @"
                    SELECT s.* 
                    FROM Skills s
                    INNER JOIN UserSkills us ON s.Id = us.SkillId
                    WHERE us.UserId = @UserId";

                await using var conn = await _dataSource.OpenConnectionAsync();
                var userSkills = await conn.QueryAsync<SkillDto>(sql, new { UserId = userId });
                return userSkills.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting skills for user: {userId}");
                return new List<SkillDto>();
            }
        }

        public async Task<List<UserDto>> GetUsersBySkillIdAsync(int skillId)
        {
            try
            {
                const string sql = @"
                    SELECT u.* 
                    FROM Users u
                    INNER JOIN UserSkills us ON u.Id = us.UserId
                    WHERE us.SkillId = @SkillId";

                await using var conn = await _dataSource.OpenConnectionAsync();
                return (await conn.QueryAsync<UserDto>(sql, new { SkillId = skillId })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users for skill: {skillId}");
                return new List<UserDto>();
            }
        }
    }

    public record SkillDto
    {
        public SkillDto(int id, string name)
        {
            Id = id;
            Name = name;
        }

        [Key]
        public int Id { get; init; }

        [Required(ErrorMessage = "Skill name is required")]
        [StringLength(255, ErrorMessage = "Skill name cannot exceed 255 characters")]
        public string Name { get; init; }
    }
}