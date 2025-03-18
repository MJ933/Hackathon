using BusinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Controllers.Classes;

namespace PresentationLayer.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly IUsersService _usersService;
       public AuthController(TokenService tokenService, IUsersService usersService)
        {
            _tokenService = tokenService;
            _usersService = usersService;
        }

        [HttpPost("LoginUserByEmailAndPassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginUserByEmailAndPassword([FromBody] LoginRequestByEmail request)
        {

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email or Password is Incorrect, please try again." });
            var inputInfo = new UserDto(1, "a", request.Email, request.Password, null, DateTime.Now);
            var user = await _usersService.GetUserByUserEmailAndPassword(inputInfo);
            if (user == null)
                return Unauthorized(new { message = "Invalid Email or Password." });
            var token = _tokenService.GenerateJwtToken(user);
            return Ok(new { Message = "Token Created successfully", Data = token });
        }
    }
    public class LoginRequestByEmail
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
