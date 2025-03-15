using DataAccessLayer;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public interface IMessagesService
    {
        Task<List<MessageDto>> GetAllMessagesAsync();
        Task<MessageDto?> GetMessageByIdAsync(int messageId);
        Task<List<MessageDto>> GetConversationAsync(int user1Id, int user2Id);
        Task<int> AddNewMessageAsync(MessageDto message);
        Task<bool> UpdateMessageAsync(MessageDto message);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<bool> IsMessageExistsByIdAsync(int messageId);
    }

    public class MessagesService : IMessagesService
    {
        private readonly IMessagesRepository _messagesRepository;

        public MessagesService(IMessagesRepository messagesRepository)
        {
            _messagesRepository = messagesRepository;
        }

        public async Task<List<MessageDto>> GetAllMessagesAsync() =>
            await _messagesRepository.GetAllMessagesAsync();

        public async Task<MessageDto?> GetMessageByIdAsync(int messageId) =>
            await _messagesRepository.GetMessageByIdAsync(messageId);

        public async Task<List<MessageDto>> GetConversationAsync(int user1Id, int user2Id) =>
            await _messagesRepository.GetConversationAsync(user1Id, user2Id);

        public async Task<int> AddNewMessageAsync(MessageDto message) =>
            await _messagesRepository.AddNewMessageAsync(message);

        public async Task<bool> UpdateMessageAsync(MessageDto message) =>
            await _messagesRepository.UpdateMessageAsync(message);

        public async Task<bool> DeleteMessageAsync(int messageId) =>
            await _messagesRepository.DeleteMessageAsync(messageId);

        public async Task<bool> IsMessageExistsByIdAsync(int messageId) =>
            await _messagesRepository.IsMessageExistsByIdAsync(messageId);
    }
}