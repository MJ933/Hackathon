using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public interface IConnectionsService
    {
        Task<List<ConnectionDto>> GetAllConnectionsAsync();
        Task<ConnectionDto?> GetConnectionByIdsAsync(int userId, int connectedUserId);
        Task<bool> AddConnectionAsync(ConnectionDto connection);
        Task<bool> UpdateConnectionStatusAsync(ConnectionDto connection);
        Task<bool> DeleteConnectionAsync(int userId, int connectedUserId);
        Task<bool> IsConnectionExistsAsync(int userId, int connectedUserId);
    }

    public class ConnectionsService : IConnectionsService
    {
        private readonly IConnectionsRepository _connectionsRepository;
        public ConnectionsService(IConnectionsRepository repository)
        {
            _connectionsRepository = repository;
        }

        public async Task<List<ConnectionDto>> GetAllConnectionsAsync() =>
            await _connectionsRepository.GetAllConnectionsAsync();

        public async Task<ConnectionDto?> GetConnectionByIdsAsync(int userId, int connectedUserId) =>
            await _connectionsRepository.GetConnectionByIdsAsync(userId, connectedUserId);

        public async Task<bool> AddConnectionAsync(ConnectionDto connection) =>
            await _connectionsRepository.AddConnectionAsync(connection);

        public async Task<bool> UpdateConnectionStatusAsync(ConnectionDto connection) =>
            await _connectionsRepository.UpdateConnectionStatusAsync(connection);

        public async Task<bool> DeleteConnectionAsync(int userId, int connectedUserId) =>
            await _connectionsRepository.DeleteConnectionAsync(userId, connectedUserId);

        public async Task<bool> IsConnectionExistsAsync(int userId, int connectedUserId) =>
            await _connectionsRepository.IsConnectionExistsAsync(userId, connectedUserId);
    }

}
