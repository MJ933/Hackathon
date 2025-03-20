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
        Task<List<InvitationDto>> GetSentInvitationsForUserPaginatedAsync(int pageNumber, int pageSize, int currentUserId);
        Task<List<InvitationDto>> GetReceivedInvitationsForUserPaginatedAsync(int pageNumber, int pageSize, int currentUserId);
        Task<List<InvitationDto>> GetInvitationsPaginatedAsync(int pageNumber, int pageSize);
    }

    public class InvitationsService : IInvitationsService
    {
        private readonly IInvitationsRepository _invitationRepository;

        public InvitationsService(IInvitationsRepository repository)
        {
            _invitationRepository = repository;
        }

        public async Task<List<InvitationDto>> GetAllInvitationsAsync() =>
            await _invitationRepository.GetAllInvitationsAsync();

        public async Task<InvitationDto?> GetInvitationByIdAsync(int invitationId) =>
            await _invitationRepository.GetInvitationByIdAsync(invitationId);
        public async Task<List<InvitationDto>> GetInvitationsPaginatedAsync(int pageNumber, int pageSize) =>
             await _invitationRepository.GetInvitationsPaginatedAsync(pageNumber, pageSize);
        public async Task<List<InvitationDto>> GetSentInvitationsForUserPaginatedAsync(int pageNumber, int pageSize, int currentUserId) =>
                await _invitationRepository.GetSentInvitationsForUserPaginatedAsync(pageNumber, pageSize, currentUserId);
        public async Task<List<InvitationDto>> GetReceivedInvitationsForUserPaginatedAsync(int pageNumber, int pageSize, int currentUserId) =>
                await _invitationRepository.GetReceivedInvitationsForUserPaginatedAsync(pageNumber, pageSize, currentUserId);
        public async Task<int> AddNewInvitationAsync(InvitationDto invitation) =>
            await _invitationRepository.AddNewInvitationAsync(invitation);

        public async Task<bool> UpdateInvitationAsync(InvitationDto invitation) =>
            await _invitationRepository.UpdateInvitationAsync(invitation);

        public async Task<bool> DeleteInvitationAsync(int invitationId) =>
            await _invitationRepository.DeleteInvitationAsync(invitationId);

        public async Task<bool> IsInvitationExistsByIdAsync(int invitationId) =>
            await _invitationRepository.IsInvitationExistsByIdAsync(invitationId);

    }
}