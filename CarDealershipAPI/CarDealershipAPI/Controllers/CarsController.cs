using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealershipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CarsController(AppDbContext db)
        {
            _db = db;
        }

        // ───── PUBLIC ENDPOINTS (no login required) ─────

        // GET: api/cars
        [HttpGet]
        public IActionResult GetAll()
        {
            var cars = _db.Cars
                .Where(c => c.IsAvailable)
                .ToList();

            return Ok(cars);
        }

        // GET: api/cars/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var car = _db.Cars.FirstOrDefault(c => c.Id == id);

            if (car == null)
                return NotFound("Car not found");

            return Ok(car);
        }

        // ───── ADMIN ONLY ENDPOINTS ─────

        // POST: api/cars
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(CarDto dto)
        {
            var car = new Car
            {
                Brand = dto.Brand,
                Model = dto.Model,
                Price = dto.Price,
                Year = dto.Year,
                IsAvailable = true // Always true when first added
            };

            _db.Cars.Add(car);
            _db.SaveChanges();

            return Ok(car);
        }

        // PUT: api/cars/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, CarDto dto)
        {
            var car = _db.Cars.FirstOrDefault(c => c.Id == id);

            if (car == null)
                return NotFound("Car not found");

            // Update the fields
            car.Brand = dto.Brand;
            car.Model = dto.Model;
            car.Price = dto.Price;
            car.Year = dto.Year;

            _db.SaveChanges();

            return Ok(car);
        }

        // DELETE: api/cars/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var car = _db.Cars.FirstOrDefault(c => c.Id == id);

            if (car == null)
                return NotFound("Car not found");

            _db.Cars.Remove(car);
            _db.SaveChanges();

            return Ok("Car deleted successfully");
        }
    }
}