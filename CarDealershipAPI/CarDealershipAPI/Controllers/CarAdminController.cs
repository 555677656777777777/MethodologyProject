using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarDealershipAPI.Controllers
{
    public class CarAdminController : Controller
    {
        private readonly AppDbContext _db;

        public CarAdminController(AppDbContext db)
        {
            _db = db;
        }

        // Check if current user is Admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // GET: /CarAdmin — List all cars (including sold)
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var cars = _db.Cars.ToList();
            ViewData["Title"] = "Manage Cars";
            return View(cars);
        }

        // GET: /CarAdmin/Create
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewData["Title"] = "Add New Car";
            return View();
        }

        // POST: /CarAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CarDto dto)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(dto);

            var car = new Car
            {
                Brand = dto.Brand,
                Model = dto.Model,
                Price = dto.Price,
                Year = dto.Year,
                IsAvailable = true
            };

            _db.Cars.Add(car);
            _db.SaveChanges();

            TempData["Success"] = $"{car.Brand} {car.Model} added successfully!";
            return RedirectToAction("Index");
        }

        // GET: /CarAdmin/Edit/5
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var car = _db.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null) return NotFound();

            var dto = new CarDto
            {
                Brand = car.Brand,
                Model = car.Model,
                Price = car.Price,
                Year = car.Year
            };

            ViewData["Title"] = $"Edit {car.Brand} {car.Model}";
            ViewData["CarId"] = car.Id;
            return View(dto);
        }

        // POST: /CarAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CarDto dto)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewData["CarId"] = id;
                return View(dto);
            }

            var car = _db.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null) return NotFound();

            car.Brand = dto.Brand;
            car.Model = dto.Model;
            car.Price = dto.Price;
            car.Year = dto.Year;

            _db.SaveChanges();

            TempData["Success"] = $"{car.Brand} {car.Model} updated!";
            return RedirectToAction("Index");
        }

        // POST: /CarAdmin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var car = _db.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null) return NotFound();

            _db.Cars.Remove(car);
            _db.SaveChanges();

            TempData["Success"] = "Car deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
