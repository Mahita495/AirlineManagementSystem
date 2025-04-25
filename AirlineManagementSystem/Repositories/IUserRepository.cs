using AirlineManagementSystem.Models;
using System.Threading.Tasks;

namespace AirlineManagementSystem.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task AddUserAsync(User user);
    }
}
