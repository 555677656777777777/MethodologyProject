using System.ComponentModel.DataAnnotations;

namespace CarDealershipAPI.DTOs
{
    public class CreateOfferDto
    {
        [Required]
        public int CarId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Offer amount must be greater than 0")]
        public decimal Amount { get; set; }
    }
}