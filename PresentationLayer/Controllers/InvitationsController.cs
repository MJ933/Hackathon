// PresentationLayer/Controllers/InvitationsController.cs
using BusinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Route("api/Invitations")]
    [ApiController]
    [Authorize]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationsService _service;

        public InvitationsController(IInvitationsService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<InvitationDto>>> GetAllInvitations()
        {
            var invitations = await _service.GetAllInvitationsAsync();
            return invitations.Count == 0
                ? NotFound("No invitations found")
                : Ok(new { Message = "Invitations retrieved successfully", Data = invitations });
        }

        [HttpGet("GetById/{id}", Name = "GetInvitationById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvitationDto>> GetInvitationById(int id)
        {
            if (id < 1) return BadRequest("Invalid invitation ID");

            var invitation = await _service.GetInvitationByIdAsync(id);
            return invitation == null
                ? NotFound($"Invitation {id} not found")
                : Ok(new { Message = "Invitation retrieved successfully", Data = invitation });
        }

        [HttpPost("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InvitationDto>> CreateInvitation([FromBody] InvitationDto invitation)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newId = await _service.AddNewInvitationAsync(invitation);
            if (newId < 0) return BadRequest("Failed to create invitation");

            //invitation = await _service.GetInvitationByIdAsync(newId)
            //    ?? new InvitationDto(newId, invitation.InviterId, invitation.InviteeId,
            //                         invitation.RoomId, invitation.Status,
            //                         DateTime.UtcNow, DateTime.UtcNow);
            invitation = invitation with { Id = newId };
            return CreatedAtRoute("GetInvitationById",
                new { id = invitation.Id },
                new { Message = "Invitation created successfully", Data = invitation });
        }

        [HttpPut("Update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvitationDto>> UpdateInvitation(int id, [FromBody] InvitationDto invitation)
        {
            if (id < 1 || !ModelState.IsValid) return BadRequest("Invalid request parameters");
            if (!await _service.IsInvitationExistsByIdAsync(id)) return NotFound($"Invitation {id} not found");

            invitation = invitation with { Id = id };
            if (!await _service.UpdateInvitationAsync(invitation))
                return BadRequest("Update failed");

            return Ok(new { Message = "Invitation updated successfully", Data = invitation });
        }

        [HttpDelete("Delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteInvitation(int id)
        {
            if (id < 1) return BadRequest("Invalid invitation ID");
            if (!await _service.IsInvitationExistsByIdAsync(id)) return NotFound($"Invitation {id} not found");
            if (!await _service.DeleteInvitationAsync(id)) return StatusCode(500, "Delete operation failed");

            return Ok(new { Message = $"Invitation {id} deleted successfully" });
        }
    }
}