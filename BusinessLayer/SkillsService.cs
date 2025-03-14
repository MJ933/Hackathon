// BusinessLayer/SkillsService.cs
using System.Threading.Tasks;
using DataAccessLayer;

namespace BusinessLayer
{
    public interface ISkillsService
    {
        Task<List<SkillDto>> GetAllSkillsAsync();
        Task<SkillDto?> GetSkillByIdAsync(int skillId);
        Task<bool> AddSkillAsync();
        Task<bool> UpdateSkillAsync();
        Task<bool> DeleteSkillAsync(int skillId);
        Task<bool> AddUserSkillAsync(int userId, int skillId);
        Task<bool> RemoveUserSkillAsync(int userId, int skillId);
        Task<List<SkillDto>> GetSkillsByUserIdAsync(int userId);
        Task<List<UserDto>> GetUsersBySkillIdAsync(int skillId);
        public SkillDto Skill { get; set; }

    }

    public class SkillsService : ISkillsService
    {
        public SkillDto Skill { get; set; }
        private readonly ISkillsRepository _skillsRepository;

        public SkillsService(ISkillsRepository skillsRepository)
        {
            _skillsRepository = skillsRepository;
        }

        public async Task<List<SkillDto>> GetAllSkillsAsync() =>
            await _skillsRepository.GetAllSkillsAsync();

        public async Task<SkillDto?> GetSkillByIdAsync(int skillId) =>
            await _skillsRepository.GetSkillByIdAsync(skillId);

        public async Task<bool> AddSkillAsync()
        {
            var InstertedSkillID = await _skillsRepository.AddSkillAsync(this.Skill);
            if (InstertedSkillID > 0)
            {
                this.Skill = new SkillDto(InstertedSkillID, this.Skill.Name);
                return InstertedSkillID > 0;
            }
            return false;
        }

        public async Task<bool> UpdateSkillAsync() =>
            await _skillsRepository.UpdateSkillAsync(this.Skill);

        public async Task<bool> DeleteSkillAsync(int skillId) =>
            await _skillsRepository.DeleteSkillAsync(skillId);

        public async Task<bool> AddUserSkillAsync(int userId, int skillId) =>
            await _skillsRepository.AddUserSkillAsync(userId, skillId);


        public async Task<bool> RemoveUserSkillAsync(int userId, int skillId) =>
            await _skillsRepository.RemoveUserSkillAsync(userId, skillId);

        public async Task<List<SkillDto>> GetSkillsByUserIdAsync(int userId) =>
            await _skillsRepository.GetSkillsByUserIdAsync(userId);

        public async Task<List<UserDto>> GetUsersBySkillIdAsync(int skillId) =>
            await _skillsRepository.GetUsersBySkillIdAsync(skillId);
    }
}