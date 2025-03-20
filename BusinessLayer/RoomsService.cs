
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer;
using DataAccessLayer.Productions;

namespace BusinessLayer
{
    public interface IRoomsService
    {
        Task<List<RoomDto>> GetAllRoomsAsync();
        Task<List<RoomDto>> GetRoomsPaginatedAsync(int pageNumber, int pageSize);
        Task<RoomDto?> GetRoomByIdAsync(int roomId);
        Task<List<UserDto>> GetUsersDetailsDataInRoomByRoomIdAsync(int roomId);
        Task<List<MessageDto>> GetMessagesInRoomByRoomIdAsync(int roomId);
        Task<List<RoomDto>> GetCurrentRoomsForCurrentUserPaginatedAsync(int pageNumber, int pageSize, int userId);

        Task<int> AddNewRoomAsync(RoomDto room);
        Task<bool> UpdateRoomAsync(RoomDto room);
        Task<bool> DeleteRoomAsync(int roomId);
        Task<bool> IsRoomExistsByIdAsync(int roomId);
        Task<List<RoomUserDto>> GetRoomUsersAsync(int roomId);
        Task<bool> AddUserToRoomAsync(int roomId, int userId);
        Task<bool> DeleteUserFromRoomAsync(int roomId, int userId);
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
        public async Task<List<RoomDto>> GetRoomsPaginatedAsync(int pageNumber, int pageSize) =>
            await _roomsRepository.GetRoomsPaginatedAsync(pageNumber, pageSize);
        public async Task<RoomDto?> GetRoomByIdAsync(int roomId) =>
            await _roomsRepository.GetRoomByIdAsync(roomId);
        public async Task<List<UserDto>> GetUsersDetailsDataInRoomByRoomIdAsync(int roomId) =>
           await _roomsRepository.GetUsersDetailsDataInRoomByRoomIdAsync(roomId);
        public async Task<List<MessageDto>> GetMessagesInRoomByRoomIdAsync(int roomId) =>
         await _roomsRepository.GetMessagesInRoomByRoomIdAsync(roomId);

        public async Task<List<RoomDto>> GetCurrentRoomsForCurrentUserPaginatedAsync(int pageNumber, int pageSize, int userId) =>
            await _roomsRepository.GetCurrentRoomsForCurrentUserPaginatedAsync(pageNumber, pageSize, userId);

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


    }
}
