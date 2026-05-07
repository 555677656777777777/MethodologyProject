using CarDealershipAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealershipAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        // GET: / — Browse available cars
        public IActionResult Index()
        {
            var cars = _db.Cars.Where(c => c.IsAvailable).ToList();
            ViewData["Title"] = "Browse Cars";
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            return View(cars);
        }

        // GET: /Home/Details/5
        public IActionResult Details(int id)
        {
            var car = _db.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null) return NotFound();
            ViewData["Title"] = $"{car.Brand} {car.Model}";
            return View(car);
        }
    }
}
