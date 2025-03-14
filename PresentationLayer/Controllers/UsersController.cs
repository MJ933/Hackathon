using BusinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _userService;

        public UsersController(IUsersService userService)
        {
            _userService = userService;
        }

        [HttpGet("GetAll", Name = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return users.Count == 0
                ? NotFound("No users found in the system")
                : Ok(new { Message = "Users retrieved successfully", Data = users });
        }

        [HttpGet("GetById/{userId}", Name = "GetUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUserById(int userId)
        {
            if (userId < 1)
                return BadRequest("Invalid user ID format");

            var user = await _userService.GetUserByUserIDAsync(userId);
            return user == null
                ? NotFound($"User with ID {userId} not found")
                : Ok(new { Message = "User retrieved successfully", Data = user });
        }

        [HttpPost("Create", Name = "CreateUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateUser([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var newUserID = await _userService.AddNewUserAsync(userDto);
            if (newUserID < 0)
                return BadRequest("Failed to create user");
            userDto = new UserDto(newUserID, userDto.Name, userDto.Email,
                                   userDto.PasswordHash, userDto.Bio, userDto.CreatedAt); // Ensure ID consistency

            return CreatedAtRoute("GetUserById",
                new { userId = newUserID },
                new { Message = "User created successfully", Data = userDto });
        }

        [HttpPut("Update/{userId}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateUser(int userId, [FromBody] UserDto userDto)
        {
            if (userId < 1 || !ModelState.IsValid)
                return BadRequest("Invalid request parameters");

            var existingUser = await _userService.GetUserByUserIDAsync(userId);
            if (existingUser == null)
                return NotFound($"User {userId} not found");

            userDto = new UserDto(userId, userDto.Name, userDto.Email,
                                    userDto.PasswordHash, userDto.Bio, userDto.CreatedAt); // Ensure ID consistency
            if (!await _userService.UpdateUserAsync(userDto))
                return BadRequest("Update operation failed");

            return Ok(new { Message = "User updated successfully", Data = userDto });
        }

        [HttpDelete("Delete/{userId}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            if (userId < 1)
                return BadRequest("Invalid user ID format");

            if (!await _userService.IsUserExistsByUserIDAsync(userId))
                return NotFound($"User {userId} not found");

            if (!await _userService.DeleteUserAsync(userId))
                return StatusCode(500, "Delete operation failed");

            return Ok(new { Message = $"User {userId} deleted successfully" });
        }
    }
}