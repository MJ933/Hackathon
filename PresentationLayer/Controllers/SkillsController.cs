using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DataAccessLayer;

namespace PresentationLayer.Controllers
{
    [Route("api/Skills")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillsService _skillsService;

        public SkillsController(ISkillsService skillsService)
        {
            _skillsService = skillsService;
        }

        [HttpGet("GetAll", Name = "GetAllSkills")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<SkillDto>>> GetAllSkills()
        {
            var skills = await _skillsService.GetAllSkillsAsync();
            return skills.Count == 0
                ? NotFound("No skills found in the system")
                : Ok(new { Message = "Skills retrieved successfully", Data = skills });
        }
        [HttpGet("GetSkillsPaginated", Name = "GetSkillsPaginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<SkillDto>>> GetSkillsPaginatedAsync(int pageNumber = 1, int pageSize = 10)
        {
            var skills = await _skillsService.GetSkillsPaginatedAsync(pageNumber, pageSize);
            return skills.Count == 0
                ? NotFound("No skills found in the system")
                : Ok(new { Message = "Skills retrieved successfully", Data = skills });
        }

        [HttpGet("GetById/{id}", Name = "GetSkillById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SkillDto>> GetSkillById(int id)
        {
            if (id < 1)
                return BadRequest("Invalid skill ID format");

            var skill = await _skillsService.GetSkillByIdAsync(id);
            return skill == null
                ? NotFound($"Skill with ID {id} not found")
                : Ok(new { Message = "Skill retrieved successfully", Data = skill });
        }

        [Authorize]
        [HttpPost("Create", Name = "CreateSkill")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SkillDto>> CreateSkill([FromBody] SkillDto skillDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int newSkillId = await _skillsService.AddSkillAsync(skillDto);
            if (newSkillId <= 0)
                return BadRequest("Failed to create skill");

            skillDto = new SkillDto(newSkillId, skillDto.Name);
            return CreatedAtRoute("GetSkillById",
                new { id = newSkillId },
                new { Message = "Skill created successfully", Data = skillDto });
        }

        [Authorize]
        [HttpPut("Update/{id}", Name = "UpdateSkill")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SkillDto>> UpdateSkill(int id, [FromBody] SkillDto skillDto)
        {
            if (id < 1 || !ModelState.IsValid)
                return BadRequest("Invalid request parameters");
            if (await _skillsService.GetSkillByIdAsync(id) == null)
                return NotFound($"Skill {id} does not exist");
            skillDto = new SkillDto(id, skillDto.Name);
            var success = await _skillsService.UpdateSkillAsync(skillDto);
            return success
                ? Ok(new { Message = "Skill updated successfully", Data = skillDto })
                : NotFound($"Skill {id} not found");
        }

        [Authorize]
        [HttpDelete("Delete/{id}", Name = "DeleteSkill")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteSkill(int id)
        {
            if (id < 1)
                return BadRequest("Invalid skill ID format");

            var success = await _skillsService.DeleteSkillAsync(id);
            return success
                ? Ok($"Skill {id} deleted successfully")
                : NotFound($"Skill {id} not found");
        }

        [Authorize]
        [HttpPost("AddUserSkill/{userId}/{skillId}", Name = "AddUserSkill")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddUserSkill(int userId, int skillId)
        {
            if (userId < 1 || skillId < 1)
                return BadRequest("Invalid user or skill ID");

            var success = await _skillsService.AddUserSkillAsync(userId, skillId);
            return success
                ? Ok("Skill added to user successfully")
                : BadRequest("Failed to add skill to user");
        }

        [Authorize]
        [HttpDelete("RemoveUserSkill/{userId}/{skillId}", Name = "RemoveUserSkill")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveUserSkill(int userId, int skillId)
        {
            if (userId < 1 || skillId < 1)
                return BadRequest("Invalid user or skill ID");

            var success = await _skillsService.RemoveUserSkillAsync(userId, skillId);
            return success
                ? Ok("Skill removed from user successfully")
                : NotFound("User-skill association not found");
        }

        [HttpGet("GetUserSkills/{userId}", Name = "GetUserSkills")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<SkillDto>>> GetSkillsByUser(int userId)
        {
            if (userId < 1)
                return BadRequest("Invalid user ID");

            var skills = await _skillsService.GetSkillsByUserIdAsync(userId);
            return Ok(new { Message = "User skills retrieved successfully", Data = skills });
        }

        [HttpGet("GetSkilledUsers/{skillId}", Name = "GetSkilledUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<UserDto>>> GetUsersBySkill(int skillId)
        {
            if (skillId < 1)
                return BadRequest("Invalid skill ID");

            var users = await _skillsService.GetUsersBySkillIdAsync(skillId);
            return Ok(new { Message = "Skilled users retrieved successfully", Data = users });
        }
    }
}