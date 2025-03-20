// BusinessLayer/SkillsService.cs
using System.Threading.Tasks;
using DataAccessLayer;

namespace BusinessLayer
{
    public interface ISkillsService
    {
        Task<List<SkillDto>> GetAllSkillsAsync();
        Task<SkillDto?> GetSkillByIdAsync(int skillId);
        Task<int> AddSkillAsync(SkillDto skill);
        Task<bool> UpdateSkillAsync(SkillDto skill);
        Task<bool> DeleteSkillAsync(int skillId);
        Task<bool> AddUserSkillAsync(int userId, int skillId);
        Task<bool> RemoveUserSkillAsync(int userId, int skillId);
        Task<List<SkillDto>> GetSkillsByUserIdAsync(int userId);
        Task<List<UserDto>> GetUsersBySkillIdAsync(int skillId);
        Task<List<SkillDto>> GetSkillsPaginatedAsync(int pageNumber, int pageSize);
    }

    public class SkillsService : ISkillsService
    {
        private readonly ISkillsRepository _skillsRepository;

        public SkillsService(ISkillsRepository skillsRepository)
        {
            _skillsRepository = skillsRepository;
        }

        public async Task<List<SkillDto>> GetAllSkillsAsync() =>
            await _skillsRepository.GetAllSkillsAsync();
        public async Task<List<SkillDto>> GetSkillsPaginatedAsync(int pageNumber, int pageSize) =>
               await _skillsRepository.GetSkillsPaginatedAsync(pageNumber, pageSize);
        public async Task<SkillDto?> GetSkillByIdAsync(int skillId) =>
            await _skillsRepository.GetSkillByIdAsync(skillId);

        public async Task<int> AddSkillAsync(SkillDto skill) =>
            await _skillsRepository.AddSkillAsync(skill);


        public async Task<bool> UpdateSkillAsync(SkillDto skill) =>
            await _skillsRepository.UpdateSkillAsync(skill);

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