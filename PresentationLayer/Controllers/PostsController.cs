using BusinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Route("api/Posts")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostsService _postsService;

        public PostsController(IPostsService service)
        {
            _postsService = service;
        }

        [HttpGet("GetAll", Name = "GetAllPosts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<PostDto>>> GetAllPosts()
        {
            var posts = await _postsService.GetAllPostsAsync();
            return posts.Count == 0
                ? NotFound("No posts found in the system")
                : Ok(new { Message = "Posts retrieved successfully", Data = posts });
        }

        [HttpGet("GetById/{postId}", Name = "GetPostById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDto>> GetPostById(int postId)
        {
            if (postId < 1)
                return BadRequest("Invalid post ID format");

            var post = await _postsService.GetPostByIdAsync(postId);
            return post == null
                ? NotFound($"Post with ID {postId} not found")
                : Ok(new { Message = "Post retrieved successfully", Data = post });
        }

        [HttpPost("Create", Name = "CreatePost")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PostDto>> CreatePost([FromBody] PostDto post)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var newPostID = await _postsService.AddNewPostAsync(post);
            if (newPostID < 0)
                return BadRequest("Failed to create post");
            post = new PostDto(newPostID, post.UserId, post.Content, post.CreatedAt);
            return CreatedAtRoute("GetPostById",
                new { postId = post.Id },
                new { Message = "Post created successfully", Data = post });
        }

        [HttpPut("Update/{postId}", Name = "UpdatePost")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDto>> UpdatePost(int postId, [FromBody] PostDto post)
        {
            if (postId < 1 || !ModelState.IsValid)
                return BadRequest("Invalid request parameters");

            if (!await _postsService.IsPostExistsByIdAsync(postId))
                return NotFound($"Post {postId} does not exist");

            post = new PostDto(postId, post.UserId, post.Content, post.CreatedAt); // Ensure ID consistency
            if (!await _postsService.UpdatePostAsync(post))
                return BadRequest("Update operation failed");

            return Ok(new { Message = "Post updated successfully", Data = post });
        }

        [HttpDelete("Delete/{postId}", Name = "DeletePost")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeletePost(int postId)
        {
            if (postId < 1)
                return BadRequest("Invalid post ID format");

            if (!await _postsService.IsPostExistsByIdAsync(postId))
                return NotFound($"Post {postId} not found");

            if (!await _postsService.DeletePostAsync(postId))
                return StatusCode(500, "Delete operation failed");

            return Ok(new { Message = $"Post {postId} deleted successfully" });
        }
    }
}