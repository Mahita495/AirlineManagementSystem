using AirlineManagementSystem.Data;
using AirlineManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AirlineManagementSystem.Repositories
{
    // Repository class to handle user-related database operations
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;  // DbContext to interact with the database
        private List<User> users;

        // Constructor to inject the AppDbContext instance
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public UserRepository(List<User> users)
        {
            this.users = users;
        }

        // Method to get a user by their username
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            // Fetches the first user that matches the provided username, or null if not found
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        // Method to add a new user to the database
        public async Task AddUserAsync(User user)
        {
            // Adds the new user entity to the Users DbSet
            _context.Users.Add(user);

            // Saves the changes to the database asynchronously
            await _context.SaveChangesAsync();
        }
    }
}
