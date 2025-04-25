using AirlineManagementSystem.Data;
using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AirlineManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Constructor that initializes the controller with necessary services (AppDbContext, IConfiguration)
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET method to display the Register page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST method for handling user registration
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
        {
            // Check if the username already exists in the database
            if (_context.Users.Any(u => u.Username == dto.Username))
            {
                ModelState.AddModelError("", "Username already exists.");  // Add error to ModelState if username exists
                return View();
            }

            // Create a new User object from the DTO
            var user = new User
            {
                Username = dto.Username,
                Role = dto.Role,
                Email = dto.Email
            };

            // Hash the user's password using PasswordHasher
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, dto.Password);

            // Add the user to the database and save changes
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Redirect to Login after successful registration
            return RedirectToAction("Login");
        }

        // GET method to display the Login page
        [HttpGet]
        public IActionResult Login(User user)
        {
            return View();
        }

        // POST method for handling user login
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest dto)
        {
            // Log the incoming login request data for debugging purposes
            Console.WriteLine($"Username: {dto?.Username}, Password: {dto?.Password}");

            // Validate that the request contains both username and password
            if (dto == null || string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
            {
                Console.WriteLine("Invalid request data");
                return BadRequest(new { error = "Invalid request data" });
            }

            // Retrieve the user from the database by username
            var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);
            if (user == null)
            {
                Console.WriteLine("Invalid username.");
                return BadRequest(new { error = "Invalid username or password" });
            }

            // Verify the password entered by the user
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, dto.Password);

            // If password verification fails, return an error message
            if (result == PasswordVerificationResult.Failed)
            {
                Console.WriteLine("Invalid password.");
                return BadRequest(new { error = "Invalid username or password" });
            }

            // Log successful login
            Console.WriteLine("User validated successfully.");

            // Create claims for JWT token based on the user's information
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username), // Username claim
                new Claim(ClaimTypes.Role, user.Role) // User role claim
            };

            // Retrieve JWT settings from configuration
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));  // Secret key for signing
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);  // Signing credentials

            // Create the JWT token with claims and expiration time
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),  // Token expires in 1 hour
                signingCredentials: creds
            );

            // Serialize the token to a string
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Debug log the token and its claims
            Console.WriteLine($"Generated token: {tokenString.Substring(0, Math.Min(50, tokenString.Length))}...");
            Console.WriteLine($"Token claims: Name={user.Username}, Role={user.Role}");

            // Return the JWT token as part of the response
            return Ok(new { token = tokenString });
        }

        // POST method for logging out the user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear the session (optional, for good measure)
            HttpContext.Session.Clear();

            // Delete the JWT cookie, if it exists
            if (Request.Cookies["jwt"] != null)
            {
                Response.Cookies.Delete("jwt");
            }

            // Redirect to the Login page
            return RedirectToAction("Login", "Auth");
        }

        // Method to handle access denied errors (e.g., unauthorized access)
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
