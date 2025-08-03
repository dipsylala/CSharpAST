using BasicDLL.Models;

namespace BasicDLL.Services
{
    /// <summary>
    /// Service interface for user management operations
    /// </summary>
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(string name, string email);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
    }

    /// <summary>
    /// In-memory implementation of user service for testing
    /// </summary>
    public class UserService : IUserService
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public async Task<User?> GetUserByIdAsync(int id)
        {
            await Task.Delay(10); // Simulate async operation
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            await Task.Delay(10);
            return _users.Where(u => u.IsActive).ToList();
        }

        public async Task<User> CreateUserAsync(string name, string email)
        {
            await Task.Delay(10);
            
            var user = new User(name, email)
            {
                Id = _nextId++
            };
            
            _users.Add(user);
            return user;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            await Task.Delay(10);
            
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null)
                return false;

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.IsActive = user.IsActive;
            existingUser.Roles = user.Roles;
            
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            await Task.Delay(10);
            
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return false;

            user.Deactivate();
            return true;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            await Task.Delay(10);
            
            return _users.Where(u => u.IsActive && u.HasRole(role)).ToList();
        }
    }
}
