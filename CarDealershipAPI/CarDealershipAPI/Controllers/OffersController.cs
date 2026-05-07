
using CarDealershipAPI.Data;
using CarDealershipAPI.DTOs;
using CarDealershipAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarDealershipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints here require login
    public class OffersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OffersController(AppDbContext db)
        {
            _db = db;
        }

        // Helper — gets the logged in user's ID from the JWT token
        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // Helper — gets the logged in user's Role from the JWT token
        private string GetUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role);
        }

        // ───── CUSTOMER: Make an offer ─────

        // POST: api/offers
        [HttpPost]
        public IActionResult CreateOffer(CreateOfferDto dto)
        {
            // Only customers can make offers
            if (GetUserRole() != "Customer")
                return Forbid();

            // Check car exists and is available
            var car = _db.Cars.FirstOrDefault(c => c.Id == dto.CarId);
            if (car == null)
                return NotFound("Car not found");

            if (!car.IsAvailable)
                return BadRequest("This car is no longer available");

            var offer = new Offer
            {
                CarId = dto.CarId,
                UserId = GetUserId(),
                Amount = dto.Amount,
                Status = "Pending" // Always starts as Pending
            };

            _db.Offers.Add(offer);
            _db.SaveChanges();

            return Ok(offer);
        }

        // ───── CUSTOMER: View their own offers ─────

        // GET: api/offers/my
        [HttpGet("my")]
        public IActionResult GetMyOffers()
        {
            var userId = GetUserId();

            var offers = _db.Offers
                .Include(o => o.Car)   // Also load the Car details
                .Where(o => o.UserId == userId)
                .ToList();

            return Ok(offers);
        }

        // ───── ADMIN: View all offers ─────

        // GET: api/offers
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllOffers()
        {
            var offers = _db.Offers
                .Include(o => o.Car)   // Load Car details
                .Include(o => o.User)  // Load User details
                .ToList();

            return Ok(offers);
        }

        // ───── ADMIN: Accept or Reject an offer ─────

        // PUT: api/offers/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateStatus(int id, UpdateOfferStatusDto dto)
        {
            var offer = _db.Offers
                .Include(o => o.Car)
                .FirstOrDefault(o => o.Id == id);

            if (offer == null)
                return NotFound("Offer not found");

            // If already processed, don't allow changing it again
            if (offer.Status != "Pending")
                return BadRequest($"This offer is already {offer.Status}");

            offer.Status = dto.Status;

            // If accepted — mark car as unavailable
            if (dto.Status == "Accepted")
            {
                offer.Car.IsAvailable = false;

                // Reject all other pending offers for the same car
                var otherOffers = _db.Offers
                    .Where(o => o.CarId == offer.CarId
                             && o.Id != offer.Id
                             && o.Status == "Pending")
                    .ToList();

                foreach (var other in otherOffers)
                    other.Status = "Rejected";
            }

            _db.SaveChanges();

            return Ok(offer);
        }
    }
}
