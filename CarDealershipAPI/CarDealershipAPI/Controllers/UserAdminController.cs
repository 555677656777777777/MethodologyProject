using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CarDealershipAPI.Controllers
{
    public class UserAdminController : Controller
    {
        private readonly AppDbContext _db;

        public UserAdminController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // GET: /UserAdmin — List all users
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

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

            ViewData["Title"] = "User Management";
            return View(users);
        }

        // GET: /UserAdmin/Details/5
        public IActionResult Details(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

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

            if (user == null) return NotFound();

            ViewData["Title"] = user.Name;
            return View(user);
        }

        // POST: /UserAdmin/Ban/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ban(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            if (user.Role == "Admin")
            {
                TempData["Error"] = "Cannot ban an Admin";
                return RedirectToAction("Index");
            }

            if (user.IsBanned)
            {
                TempData["Error"] = "User is already banned";
                return RedirectToAction("Index");
            }

            user.IsBanned = true;
            _db.SaveChanges();

            TempData["Success"] = $"{user.Name} has been banned";
            return RedirectToAction("Index");
        }

        // POST: /UserAdmin/Unban/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Unban(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            if (!user.IsBanned)
            {
                TempData["Error"] = "User is not banned";
                return RedirectToAction("Index");
            }

            user.IsBanned = false;
            _db.SaveChanges();

            TempData["Success"] = $"{user.Name} has been unbanned";
            return RedirectToAction("Index");
        }
    }
}
