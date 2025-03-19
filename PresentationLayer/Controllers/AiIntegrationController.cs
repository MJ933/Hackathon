using BusinessLayer;
using BusinessLayer.Client;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiIntegrationController : ControllerBase
    {
        private readonly GeminiApiClientService geminiApiClient;

        public AiIntegrationController(
            GeminiApiClientService geminiApiClient)
        {
            this.geminiApiClient = geminiApiClient;
        }
        [HttpPost]
        [Route("{userId:int}/SuggestOpenSourceProjects")]
        public async Task<IActionResult> SuggestOpenSourceProjectsAsync(int userId, string? message = null)
        {
            var result = await geminiApiClient.SuggestOpenSourceProjectsAsync(userId);

            return Ok(result);
        }

        [HttpPost]
        [Route("{roomId:int}/SuggestRoomProject")]
        public async Task<IActionResult> SuggestProjectForRoomAsync(int roomId, string? message = null)
        {
            var result = await geminiApiClient.SuggestProjectForRoomAsync(roomId);

            return Ok(result);
        }
    }
}
