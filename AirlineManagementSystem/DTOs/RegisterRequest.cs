using System.ComponentModel.DataAnnotations;

namespace AirlineManagementSystem.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(4, ErrorMessage = "Username must be at least 4 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Manager|User)$", ErrorMessage = "Role must be either 'Manager' or 'User'")]
        public string Role { get; set; }
    }
}
