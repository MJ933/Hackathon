using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface IUsersRepository
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByUserIDAsync(int userID);
        Task<int> AddNewUserAsync(UserDto user);
        Task<bool> UpdateUserAsync(UserDto user);
        Task<bool> DeleteUserAsync(int userID);
        Task<bool> IsUserExistsByUserIDAsync(int userID);
    }
    public class UsersRepository : IUsersRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<UsersRepository> _logger;

        public UsersRepository(NpgsqlDataSource dataSource, ILogger<UsersRepository> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                await using var conn = await _dataSource.OpenConnectionAsync();
                var result = await conn.QueryAsync<UserDto>("Select * from Users");
                return result.ToList();
            }
            catch (Exception ex)
            {
                {
                    _logger.LogError(ex, "Error in GetAllUsersAsync");
                    return new List<UserDto>();
                }
            }
        }

        public async Task<UserDto?> GetUserByUserIDAsync(int userID)
        {
            try
            {
                const string sql = "Select * from Users where Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var user = await conn.QuerySingleOrDefaultAsync<UserDto>(sql, new { Id = userID });
                return user;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetUserByUserID for UserID: {userID}");
                return null;
            }
        }

        public async Task<int> AddNewUserAsync(UserDto user)
        {
            try
            {
                const string sql = @"Insert into Users
                                    (Name,Email,PasswordHash,Bio,CreatedAt)
                                    Values 
                                    (@Name,@Email,@PasswordHash,@Bio,@CreatedAt)
                                    returning id";
                var parameters = new
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PasswordHash = user.PasswordHash,
                    Bio = user.Bio,
                    CreatedAt = user.CreatedAt
                };
                await using var conn = await _dataSource.OpenConnectionAsync();
                var InsertedUserID = await conn.ExecuteScalarAsync<int>(sql, parameters);
                return InsertedUserID;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AddNewUserAsync for Name: {user.Name}");
                return -1;
            }
        }

        public async Task<bool> UpdateUserAsync(UserDto user)
        {
            try
            {
                const string sql = @"
                                    Update Users
                                    set
                                        Name=@Name,
                                        Email=@Email,
                                        PasswordHash=@PasswordHash,
                                        Bio=@Bio,
                                        CreatedAt=@CreatedAt
                                    Where Id = @Id";
                var parameters = new
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PasswordHash = user.PasswordHash,
                    Bio = user.Bio,
                    CreatedAt = user.CreatedAt
                };
                await using var conn = await _dataSource.OpenConnectionAsync();
                var AffectedRows = await conn.ExecuteAsync(sql, parameters);
                return AffectedRows > 0;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdateUserAsync for Name: {user.Name}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userID)
        {
            try
            {
                const string sql = @"Delete From Users Where Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { Id = userID });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DeleteUserAsync for UserID: {userID}");
                return false;
            }
        }

        public async Task<bool> IsUserExistsByUserIDAsync(int UserID)
        {
            try
            {
                const string sql = "Select 1 from Users Where Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();

                int result = await conn.QueryFirstOrDefaultAsync<int>(sql, new { Id = UserID });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in IsUserExistsByUserIDAsync for UserID: {UserID}");
                return false;
            }
        }
    }
    public record UserDto
    {
        public UserDto(int id, string name, string email, string passwordHash, string? bio, DateTime createdAt)
        {
            this.Id = id;
            this.Name = name;
            this.Email = email;
            this.PasswordHash = passwordHash;
            this.Bio = bio;
            this.CreatedAt = createdAt;

        }
        // Primary key, auto-generated by the database
        [Key] // Indicates this is the primary key
        public int Id { get; init; } = 0;

        // Required field with maximum length of 255 characters
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string Name { get; init; } = string.Empty;

        // Required field with email format validation
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; init; } = string.Empty;

        // Required field with maximum length of 255 characters
        [Required(ErrorMessage = "Password hash is required.")]
        [StringLength(255, ErrorMessage = "Password hash cannot exceed 255 characters.")]
        public string PasswordHash { get; init; } = string.Empty;

        // Optional field with maximum length (if needed)
        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters.")]
        public string? Bio { get; init; } = null;

        // Timestamp with default value
        [DataType(DataType.DateTime)] // Specifies the data type for validation
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }

}
