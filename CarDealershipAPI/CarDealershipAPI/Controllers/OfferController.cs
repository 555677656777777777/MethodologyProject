

using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealershipAPI.Controllers
{
    public class OfferController : Controller
    {
        private readonly AppDbContext _db;

        public OfferController(AppDbContext db)
        {
            _db = db;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private string GetUserRole() => HttpContext.Session.GetString("UserRole") ?? "";

        // ═══ CUSTOMER: Create an offer ═══

        // GET: /Offer/Create/5  (carId)
        public IActionResult Create(int id)
        {
            if (GetUserId() == null) return RedirectToAction("Login", "Account");
            if (GetUserRole() != "Customer")
            {
                TempData["Error"] = "Only customers can make offers";
                return RedirectToAction("Index", "Home");
            }

            var car = _db.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null) return NotFound();

            if (!car.IsAvailable)
            {
                TempData["Error"] = "This car is no longer available";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = $"Make Offer — {car.Brand} {car.Model}";
            ViewData["Car"] = car;
            return View();
        }

        // POST: /Offer/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int id, CreateOfferDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");
            if (GetUserRole() != "Customer")
            {
                TempData["Error"] = "Only customers can make offers";
                return RedirectToAction("Index", "Home");
            }

            var car = _db.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null) return NotFound();

            if (!car.IsAvailable)
            {
                TempData["Error"] = "This car is no longer available";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Car"] = car;
                return View(dto);
            }

            var offer = new Offer
            {
                CarId = id,
                UserId = userId.Value,
                Amount = dto.Amount,
                Status = "Pending"
            };

            _db.Offers.Add(offer);
            _db.SaveChanges();

            TempData["Success"] = "Offer submitted successfully!";
            return RedirectToAction("MyOffers");
        }

        // ═══ CUSTOMER: View my offers ═══

        // GET: /Offer/MyOffers
        public IActionResult MyOffers()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var offers = _db.Offers
                .Include(o => o.Car)
                .Where(o => o.UserId == userId.Value)
                .ToList();

            ViewData["Title"] = "My Offers";
            return View(offers);
        }

        // ═══ ADMIN: View all offers ═══
        


        // ═══ ADMIN: Accept / Reject offer ═══

        // POST: /Offer/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string status)
        {
            if (GetUserRole() != "Admin") return RedirectToAction("Login", "Account");

            var offer = _db.Offers
                .Include(o => o.Car)
                .FirstOrDefault(o => o.Id == id);

            if (offer == null) return NotFound();

            if (offer.Status != "Pending")
            {
                TempData["Error"] = $"This offer is already {offer.Status}";
                return RedirectToAction("AllOffers");
            }

            offer.Status = status;

            // If accepted — mark car as unavailable and reject other offers
            if (status == "Accepted")
            {
                offer.Car.IsAvailable = false;

                var otherOffers = _db.Offers
                    .Where(o => o.CarId == offer.CarId
                            && o.Id != offer.Id
                            && o.Status == "Pending")
                    .ToList();

                foreach (var other in otherOffers)
                    other.Status = "Rejected";
            }

            _db.SaveChanges();

            TempData["Success"] = $"Offer {status.ToLower()} successfully!";
            return RedirectToAction("AllOffers");
        }
    }
}    

