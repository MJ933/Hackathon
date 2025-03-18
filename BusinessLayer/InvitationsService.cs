// BusinessLayer/InvitationsService.cs
using DataAccessLayer;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public interface IInvitationsService
    {
        Task<List<InvitationDto>> GetAllInvitationsAsync();
        Task<InvitationDto?> GetInvitationByIdAsync(int invitationId);
        Task<int> AddNewInvitationAsync(InvitationDto invitation);
        Task<bool> UpdateInvitationAsync(InvitationDto invitation);
        Task<bool> DeleteInvitationAsync(int invitationId);
        Task<bool> IsInvitationExistsByIdAsync(int invitationId);
    }

    public class InvitationsService : IInvitationsService
    {
        private readonly IInvitationsRepository _repository;

        public InvitationsService(IInvitationsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<InvitationDto>> GetAllInvitationsAsync() =>
            await _repository.GetAllInvitationsAsync();

        public async Task<InvitationDto?> GetInvitationByIdAsync(int invitationId) =>
            await _repository.GetInvitationByIdAsync(invitationId);

        public async Task<int> AddNewInvitationAsync(InvitationDto invitation) =>
            await _repository.AddNewInvitationAsync(invitation);

        public async Task<bool> UpdateInvitationAsync(InvitationDto invitation) =>
            await _repository.UpdateInvitationAsync(invitation);

        public async Task<bool> DeleteInvitationAsync(int invitationId) =>
            await _repository.DeleteInvitationAsync(invitationId);

        public async Task<bool> IsInvitationExistsByIdAsync(int invitationId) =>
            await _repository.IsInvitationExistsByIdAsync(invitationId);
    }
}