﻿using BusinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Route("api/Messages")]
    [ApiController]
    [Authorize]

    public class MessagesController : ControllerBase
    {
        private readonly IMessagesService _messagesService;
        private readonly IUsersService _usersService;

        public MessagesController(IMessagesService messagesService, IUsersService usersService)
        {
            _messagesService = messagesService;
            _usersService = usersService;
        }
        [HttpGet("GetAll", Name = "GetAllMessages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<MessageDto>>> GetAllMessages()
        {
            var messages = await _messagesService.GetAllMessagesAsync();
            return messages.Count == 0
                ? NotFound("No messages found in the system")
                : Ok(new { Message = "Messages retrieved successfully", Data = messages });
        }

        [HttpGet("GetById/{messageId}", Name = "GetMessageById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MessageDto>> GetMessageById(int messageId)
        {
            if (messageId < 1)
                return BadRequest("Invalid message ID format");

            var message = await _messagesService.GetMessageByIdAsync(messageId);
            return message == null
                ? NotFound($"Message with ID {messageId} not found")
                : Ok(new { Message = "Message retrieved successfully", Data = message });
        }

        [HttpGet("Conversation/{user1Id}/{user2Id}", Name = "GetConversation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<MessageDto>>> GetConversation(int user1Id, int user2Id)
        {
            if (user1Id < 1 || user2Id < 1)
                return BadRequest("Invalid user ID format");

            var messages = await _messagesService.GetConversationAsync(user1Id, user2Id);
            return messages.Count == 0
                ? NotFound("No messages found between the users")
                : Ok(new { Message = "Conversation retrieved successfully", Data = messages });
        }

        [HttpPost("Create", Name = "CreateMessage")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MessageDto>> CreateMessage([FromBody] MessageDto message)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (message.SenderId < 0 || message.ReceiverId < 0)
                return BadRequest("Invalid user ID format");
            // make the receiverId for the user as null if the ReceiverId is 0.
            if (message.ReceiverId == 0)
                message = message with { ReceiverId = null };

            // check if the user exist.
            if (!await _usersService.IsUserExistsByUserIDAsync(message.SenderId))
                return BadRequest($"User sender Does not exists with userID: {message.SenderId}");
            // check if the user sender is null and also check if the user sender exist.
            if (message.ReceiverId != null && !await _usersService.IsUserExistsByUserIDAsync(message.ReceiverId))
                return BadRequest($"User receiver Does not exists with userID: {message.ReceiverId}");


            var newMessageId = await _messagesService.AddNewMessageAsync(message);
            if (newMessageId < 0)
                return BadRequest("Failed to create message");
            // update the message Id with the New Id.
            message = message with { Id = newMessageId };
            return CreatedAtRoute("GetMessageById", new { messageId = message.Id },
                new { Message = "Message created successfully", Data = message });
        }

        [HttpPut("Update/{messageId}", Name = "UpdateMessage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MessageDto>> UpdateMessage(int messageId, [FromBody] MessageDto message)
        {
            if (messageId < 1 || !ModelState.IsValid)
                return BadRequest("Invalid request parameters");

            if (!await _messagesService.IsMessageExistsByIdAsync(messageId))
                return NotFound($"Message {messageId} does not exist");

            message = message with { Id = messageId };
            if (!await _messagesService.UpdateMessageAsync(message))
                return BadRequest("Update operation failed");

            return Ok(new { Message = "Message updated successfully", Data = message });
        }

        [HttpDelete("Delete/{messageId}", Name = "DeleteMessage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteMessage(int messageId)
        {
            if (messageId < 1)
                return BadRequest("Invalid message ID format");

            if (!await _messagesService.IsMessageExistsByIdAsync(messageId))
                return NotFound($"Message {messageId} not found");

            if (!await _messagesService.DeleteMessageAsync(messageId))
                return StatusCode(500, "Delete operation failed");

            return Ok(new { Message = $"Message {messageId} deleted successfully" });
        }
    }
}