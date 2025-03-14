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
        Task<bool> AddNewUserAsync();
        Task<bool> UpdateUserAsync();
        Task<bool> DeleteUserAsync(int userID);
        Task<bool> IsUserExistsByUserIDAsync(int userID);
        public UserDto User { get; set; }
    }
    public class UsersService : IUsersService
    {
        public UserDto User { get; set; }
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

        public async Task<bool> AddNewUserAsync()
        {
            int InsertedUserID = await _userRepository.AddNewUserAsync(this.User);
            if (InsertedUserID > 0)
            {
                this.User = new UserDto(InsertedUserID, this.User.Name, this.User.Email,
                    this.User.PasswordHash, this.User.Bio, this.User.CreatedAt);
                return InsertedUserID > 0;
            }
            return false;
        }
        public async Task<bool> UpdateUserAsync()
        {
            return await _userRepository.UpdateUserAsync(this.User);
        }
        public async Task<bool> DeleteUserAsync(int userID)
        {
            return await _userRepository.DeleteUserAsync(userID);
        }
        public async Task<bool> IsUserExistsByUserIDAsync(int orderID)
        {
            return await _userRepository.IsUserExistsByUserIDAsync(orderID);
        }

    }
}
