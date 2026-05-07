using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarDealershipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            // Check if email already exists
            if (_db.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already registered");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Customer",
                IsBanned = false
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok("Registration successful");
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            // Find user by email
            var user = _db.Users.FirstOrDefault(u => u.Email == dto.Email);

            // Validate user and password
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Invalid email or password");

            if (user.IsBanned)
                return Unauthorized("Your account has been banned");

            // Generate JWT token
            var token = GenerateToken(user);

            return Ok(new
            {
                token = token,
                name = user.Name,
                role = user.Role
            });
        }

        // Private helper — builds the JWT token
        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            // Claims are pieces of info stored inside the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8), // Token lasts 8 hours
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}