using CarDealershipAPI.Models;

namespace CarDealershipAPI.Models
{
    public class Offer
    {
        public int Id { get; set; }
        public int CarId { get; set; }          // Which car is this offer for?
        public int UserId { get; set; }         // Who made the offer?
        public decimal Amount { get; set; }     // How much did they offer?
        public string Status { get; set; }      // "Pending", "Accepted", "Rejected"

        // Navigation properties (Entity Framework uses these)
        public Car Car { get; set; }
        public User User { get; set; }
    }
}