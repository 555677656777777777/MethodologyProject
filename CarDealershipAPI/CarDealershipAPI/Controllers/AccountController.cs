using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarDealershipAPI.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            ViewData["Title"] = "Sign In";
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = _db.Users.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                TempData["Error"] = "Invalid email or password";
                return View(dto);
            }

            if (user.IsBanned)
            {
                TempData["Error"] = "Your account has been banned";
                return View(dto);
            }

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role);

            TempData["Success"] = $"Welcome back, {user.Name}!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            ViewData["Title"] = "Create Account";
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (_db.Users.Any(u => u.Email == dto.Email))
            {
                TempData["Error"] = "Email already registered";
                return View(dto);
            }

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

            TempData["Success"] = "Registration successful! Please sign in.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out";
            return RedirectToAction("Index", "Home");
        }
    }
}
