using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public interface IUsersService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByUserIDAsync(int userID);
        Task<int> AddNewUserAsync(UserDto user);
        Task<bool> UpdateUserAsync(UserDto user);
        Task<bool> DeleteUserAsync(int userID);
        Task<bool> IsUserExistsByUserIDAsync(int? userID);
        Task<UserDto?> GetUserByUserEmailAndPassword(UserDto userDto);
        Task<(List<UserDto> usersList, int totalCount)> GetUsersPaginatedWithFiltersAsync(int pageNumber, int pageSize, int? id,
            string? name, string? email, string? bio, DateTime? createdAt);
        //public UserDto User { get; set; }
    }
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _userRepository;
        public UsersService(IUsersRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<UserDto?> GetUserByUserIDAsync(int userID)
        {
            return await _userRepository.GetUserByUserIDAsync(userID);
        }

        public async Task<int> AddNewUserAsync(UserDto user) =>
        await _userRepository.AddNewUserAsync(user);


        public async Task<bool> UpdateUserAsync(UserDto user)
        {
            return await _userRepository.UpdateUserAsync(user);
        }
        public async Task<bool> DeleteUserAsync(int userID)
        {
            return await _userRepository.DeleteUserAsync(userID);
        }
        public async Task<bool> IsUserExistsByUserIDAsync(int? orderID)
        {
            return await _userRepository.IsUserExistsByUserIDAsync(orderID);
        }
        public async Task<UserDto?> GetUserByUserEmailAndPassword(UserDto userDto) =>
             await _userRepository.GetUserByUserEmailAndPassword(userDto);

        public async Task<(List<UserDto> usersList, int totalCount)> GetUsersPaginatedWithFiltersAsync(int pageNumber, int pageSize, int? id,
             string? name, string? email, string? bio, DateTime? createdAt) =>
             await _userRepository.GetUsersPaginatedWithFiltersAsync(pageNumber, pageSize, id, name, email, bio, createdAt);

    }
}
