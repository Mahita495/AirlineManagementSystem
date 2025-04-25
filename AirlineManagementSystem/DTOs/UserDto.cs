namespace AirlineManagementSystem.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; } // Manager or User
        public string Email { get; set; }
    }
}
