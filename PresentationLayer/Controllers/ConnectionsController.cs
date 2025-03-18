using BusinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Route("api/Connections")]
    [ApiController]
    [Authorize]
    public class ConnectionsController : ControllerBase
    {
        private readonly IConnectionsService _connectionsService;

        public ConnectionsController(IConnectionsService service)
        {
            _connectionsService = service;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<ConnectionDto>>> GetAllConnections()
        {
            var connections = await _connectionsService.GetAllConnectionsAsync();
            return connections.Count == 0
                ? NotFound("No connections found")
                : Ok(new { Message = "Connections retrieved successfully", Data = connections });
        }

        [HttpGet("GetByIds/{userId}/{connectedUserId}", Name = "GetConnectionByIds")]
        public async Task<ActionResult<ConnectionDto>> GetConnectionByIds(int userId, int connectedUserId)
        {
            if (userId < 1 || connectedUserId < 1)
                return BadRequest("Invalid user ID(s)");

            var connection = await _connectionsService.GetConnectionByIdsAsync(userId, connectedUserId);
            return connection == null
                ? NotFound($"Connection between {userId} and {connectedUserId} not found")
                : Ok(new { Message = "Connection retrieved successfully", Data = connection });
        }

        [HttpPost("Create")]
        public async Task<ActionResult<ConnectionDto>> CreateConnection([FromBody] ConnectionDto connection)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _connectionsService.IsConnectionExistsAsync(connection.UserId, connection.ConnectedUserId))
                return BadRequest("Connection already exists");

            if (!await _connectionsService.AddConnectionAsync(connection))
                return StatusCode(500, "Failed to create connection");

            return CreatedAtRoute(
                "GetConnectionByIds",
                new { userId = connection.UserId, connectedUserId = connection.ConnectedUserId },
                new { Message = "Connection created successfully", Data = connection }
            );
        }

        [HttpPut("UpdateStatus/{userId}")]
        public async Task<ActionResult<ConnectionDto>> UpdateConnectionStatus(int userId, [FromBody] ConnectionDto connection)
        {
            if (connection.UserId < 1 || connection.ConnectedUserId < 1)
                return BadRequest("Invalid user ID(s)");

            if (!await _connectionsService.IsConnectionExistsAsync(userId, connection.ConnectedUserId))
                return NotFound("Connection not found");
            connection = new ConnectionDto(userId, connection.ConnectedUserId, connection.Status, connection.CreatedAt);
            if (!await _connectionsService.UpdateConnectionStatusAsync(connection))
                return StatusCode(500, "Failed to update connection status");

            var updatedConnection = await _connectionsService.GetConnectionByIdsAsync(userId, connection.ConnectedUserId);
            return Ok(new { Message = "Connection status updated successfully", Data = updatedConnection });
        }

        [HttpDelete("Delete/{userId}/{connectedUserId}")]
        public async Task<ActionResult> DeleteConnection(int userId, int connectedUserId)
        {
            if (userId < 1 || connectedUserId < 1)
                return BadRequest("Invalid user ID(s)");

            if (!await _connectionsService.IsConnectionExistsAsync(userId, connectedUserId))
                return NotFound("Connection not found");

            if (!await _connectionsService.DeleteConnectionAsync(userId, connectedUserId))
                return StatusCode(500, "Failed to delete connection");

            return Ok(new { Message = $"Connection between {userId} and {connectedUserId} deleted successfully" });
        }
    }
}