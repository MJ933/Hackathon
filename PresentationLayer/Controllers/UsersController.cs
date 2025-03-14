using BusinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    //[Route("api/[controller]")]
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
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var usersList = await _userService.GetAllUsersAsync();
            if (usersList.Count == 0)
                return NotFound("There are no users in the database!");
            return Ok(usersList);
        }

        [HttpGet("GetUserByID/{UserID}", Name = "GetUserByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserByID([FromRoute] int UserID)
        {
            if (UserID < 1)
                return BadRequest($"Invalid UserID: {UserID}");
            var UserDTO = await _userService.GetUserByUserIDAsync(UserID);
            if (UserDTO == null)
                return NotFound($"No User found with UserID: {UserID}");
            return Ok(UserDTO);
        }




        [HttpPost("Create", Name = "AddUser")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<UserDto>> AddUser([FromBody] UserDto newUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _userService.User = newUserDTO;
            if (await _userService.AddNewUserAsync())
            {
                return CreatedAtRoute("GetUserByID", new { UserID = _userService.User.Id }, _userService.User);
            }
            else
            {
                return BadRequest("Failed to add the User.");
            }
        }

        [HttpPut("Update/{UserID}", Name = "UpdateUserAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> UpdateUser([FromRoute] int UserID, [FromBody] UserDto updatedUserDTO)
        {
            if (UserID < 1)
                return BadRequest($"Invalid ID: {UserID}");
            updatedUserDTO = new UserDto(UserID, updatedUserDTO.Name, updatedUserDTO.Email,
                     updatedUserDTO.PasswordHash, updatedUserDTO.Bio, updatedUserDTO.CreatedAt);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            UserDto? oldUserDto = await _userService.GetUserByUserIDAsync(UserID);
            if (oldUserDto == null)
                return NotFound($"No User found with ID: {UserID}");
            _userService.User = updatedUserDTO;
            if (await _userService.UpdateUserAsync())
            {
                return Ok(updatedUserDTO);
            }
            else
            {
                return BadRequest("Failed to update the User.");
            }
        }

        [HttpDelete("Delete/{UserID}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUser([FromRoute] int UserID)
        {
            if (UserID <= 0)
                return BadRequest($"Invalid ID: {UserID}");
            if (!await _userService.IsUserExistsByUserIDAsync(UserID))
                return NotFound($"No User found with ID: {UserID}");
            if (await _userService.DeleteUserAsync(UserID))
                return Ok($"User with ID: {UserID} was deleted successfully.");
            else
                return StatusCode(500, "Failed to delete the User.");
        }
    }
}
