using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarDealershipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // All endpoints here are Admin only
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/users
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _db.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsBanned = u.IsBanned
                })
                .ToList();

            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _db.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsBanned = u.IsBanned
                })
                .FirstOrDefault();

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        // PUT: api/users/5/ban
        [HttpPut("{id}/ban")]
        public IActionResult Ban(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            // Prevent banning other Admins
            if (user.Role == "Admin")
                return BadRequest("Cannot ban an Admin");

            if (user.IsBanned)
                return BadRequest("User is already banned");

            user.IsBanned = true;
            _db.SaveChanges();

            return Ok($"{user.Name} has been banned");
        }

        // PUT: api/users/5/unban
        [HttpPut("{id}/unban")]
        public IActionResult Unban(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            if (!user.IsBanned)
                return BadRequest("User is not banned");

            user.IsBanned = false;
            _db.SaveChanges();

            return Ok($"{user.Name} has been unbanned");
        }
    }
}
