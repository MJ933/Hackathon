using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface IPostsRepository
    {
        Task<List<PostDto>> GetAllPostsAsync();
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task<int> AddNewPostAsync(PostDto post);
        Task<bool> UpdatePostAsync(PostDto post);
        Task<bool> DeletePostAsync(int postId);
        Task<bool> IsPostExistsByIdAsync(int postId);
    }

    public class PostsRepository : IPostsRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<PostsRepository> _logger;

        public PostsRepository(NpgsqlDataSource dataSource, ILogger<PostsRepository> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<List<PostDto>> GetAllPostsAsync()
        {
            try
            {
                await using var conn = await _dataSource.OpenConnectionAsync();
                var posts = await conn.QueryAsync<PostDto>("SELECT * FROM Posts order by Id desc");
                return posts.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all posts");
                return new List<PostDto>();
            }
        }

        public async Task<PostDto?> GetPostByIdAsync(int postId)
        {
            try
            {
                const string sql = "SELECT * FROM Posts WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var post = await conn.QuerySingleOrDefaultAsync<PostDto>(sql, new { Id = postId });
                return post;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching post {postId}");
                return null;
            }
        }

        public async Task<int> AddNewPostAsync(PostDto post)
        {
            try
            {
                const string sql = @"
                    INSERT INTO Posts (UserId, Content, CreatedAt)
                    VALUES (@UserId, @Content, @CreatedAt)
                    RETURNING Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                int LastInsertedPostID = await conn.ExecuteScalarAsync<int>(sql, post);
                return LastInsertedPostID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding post for user {post.UserId}");
                return -1;
            }
        }

        public async Task<bool> UpdatePostAsync(PostDto post)
        {
            try
            {
                const string sql = @"
                    UPDATE Posts
                    SET UserId = @UserId, Content = @Content
                    WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, post);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating post {post.Id}");
                return false;
            }
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            try
            {
                const string sql = "DELETE FROM Posts WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.ExecuteAsync(sql, new { Id = postId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting post {postId}");
                return false;
            }
        }

        public async Task<bool> IsPostExistsByIdAsync(int postId)
        {
            try
            {
                const string sql = "SELECT 1 FROM Posts WHERE Id = @Id";
                await using var conn = await _dataSource.OpenConnectionAsync();
                var rowsAffected = await conn.QueryFirstOrDefaultAsync<int>(sql, new { Id = postId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking post {postId} existence");
                return false;
            }
        }
    }

    public record PostDto
    {
        public PostDto(int id, int userId, string content, DateTime createdAt)
        {
            Id = id;
            UserId = userId;
            Content = content;
            CreatedAt = createdAt;
        }

        [Key]
        public int Id { get; init; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; init; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}