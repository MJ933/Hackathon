
using System.Threading.Tasks;
using DataAccessLayer;

namespace BusinessLayer
{
    public interface IRoomsService
    {
        Task<List<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto?> GetRoomByIdAsync(int roomId);
        Task<int> AddNewRoomAsync(RoomDto room);
        Task<bool> UpdateRoomAsync(RoomDto room);
        Task<bool> DeleteRoomAsync(int roomId);
        Task<bool> IsRoomExistsByIdAsync(int roomId);
        Task<List<RoomUserDto>> GetRoomUsersAsync(int roomId);
        Task<bool> AddUserToRoomAsync(int roomId, int userId);
        Task<bool> DeleteUserFromRoomAsync(int roomId, int userId);
        Task<List<UserDto>> GetUsersDetailsDataInRoomByRoomIdAsync(int roomId);
        Task<List<MessageDto>> GetMessagesInRoomByRoomIdAsync(int roomId);
    }
    public class RoomsService : IRoomsService
    {
        private readonly IRoomsRepository _roomsRepository;

        public RoomsService(IRoomsRepository repository)
        {
            _roomsRepository = repository;
        }

        public async Task<List<RoomDto>> GetAllRoomsAsync() =>
            await _roomsRepository.GetAllRoomsAsync();

        public async Task<RoomDto?> GetRoomByIdAsync(int roomId) =>
            await _roomsRepository.GetRoomByIdAsync(roomId);

        public async Task<int> AddNewRoomAsync(RoomDto room) =>
            await _roomsRepository.AddNewRoomAsync(room);

        public async Task<bool> UpdateRoomAsync(RoomDto room) =>
            await _roomsRepository.UpdateRoomAsync(room);

        public async Task<bool> DeleteRoomAsync(int roomId) =>
            await _roomsRepository.DeleteRoomAsync(roomId);

        public async Task<bool> IsRoomExistsByIdAsync(int roomId) =>
            await _roomsRepository.IsRoomExistsByIdAsync(roomId);

        public async Task<List<RoomUserDto>> GetRoomUsersAsync(int roomId) =>
            await _roomsRepository.GetRoomUsersAsync(roomId);

        public async Task<bool> AddUserToRoomAsync(int roomId, int userId) =>
            await _roomsRepository.AddUserToRoomAsync(roomId, userId);

        public async Task<bool> DeleteUserFromRoomAsync(int roomId, int userId) =>
            await _roomsRepository.DeleteUserFromRoomAsync(roomId, userId);

        public async Task<List<UserDto>> GetUsersDetailsDataInRoomByRoomIdAsync(int roomId) =>
             await _roomsRepository.GetUsersDetailsDataInRoomByRoomIdAsync(roomId);
        public async Task<List<MessageDto>> GetMessagesInRoomByRoomIdAsync(int roomId) =>
         await _roomsRepository.GetMessagesInRoomByRoomIdAsync(roomId);
    }
}
