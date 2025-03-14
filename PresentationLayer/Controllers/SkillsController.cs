// PresentationLayer/Controllers/SkillsController.cs
using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
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
        public async Task<ActionResult<List<SkillDto>>> GetAllSkills()
        {
            var skills = await _skillsService.GetAllSkillsAsync();
            return Ok(skills);
        }

        [HttpGet("GetSkillBy/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SkillDto>> GetSkillById(int id)
        {
            var skill = await _skillsService.GetSkillByIdAsync(id);
            return skill != null ? Ok(skill) : NotFound();
        }

        [HttpPost("CreateSkill")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SkillDto>> CreateSkill([FromBody] SkillDto skillDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _skillsService.Skill = skillDto;
            if (!await _skillsService.AddSkillAsync())
                return BadRequest("Failed to create skill");
            skillDto = new SkillDto(_skillsService.Skill.Id, skillDto.Name);
            return CreatedAtAction(nameof(GetSkillById), new { id = _skillsService.Skill.Id }, skillDto);
        }

        [HttpPut("UpdateSkill/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateSkill(int id, [FromBody] SkillDto skillDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            _skillsService.Skill = new SkillDto(id, skillDto.Name);
            var success = await _skillsService.UpdateSkillAsync();
            return success ? Ok( _skillsService.Skill) : BadRequest("Update failed");
        }

        [HttpDelete("DeleteSkill/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult> DeleteSkill(int id)
        {
            var success = await _skillsService.DeleteSkillAsync(id);
            return success ? Ok("Skill Deleted successfully") : NotFound("Failed to remove the skill");
        }

        [HttpPost("AddUserSkill/{userId}/{skillId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddUserSkill(int userId, int skillId)
        {
            if (userId < 1 || skillId < 1)
                return BadRequest("Invalid IDs");

            var success = await _skillsService.AddUserSkillAsync(userId, skillId);
            return success ? Ok("skill has been Added successfully") : BadRequest("Failed to add skill to user");
        }

        [HttpDelete("RemoveUserSkill/{userId}/{skillId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RemoveUserSkill(int userId, int skillId)
        {
            if (userId < 1 || skillId < 1)
                return BadRequest("Invalid IDs");

            var success = await _skillsService.RemoveUserSkillAsync(userId, skillId);
            return success ? Ok("Skill has been removed correctly.") : NotFound("Failed to remove the skill");
        }

        [HttpGet("GetSkillsByUser/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SkillDto>>> GetSkillsByUser(int userId)
        {
            var skills = await _skillsService.GetSkillsByUserIdAsync(userId);
            return Ok(skills);
        }

        [HttpGet("GetUsersBySkill/{skillId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserDto>>> GetUsersBySkill(int skillId)
        {
            var users = await _skillsService.GetUsersBySkillIdAsync(skillId);
            return Ok(users);
        }
    }
}