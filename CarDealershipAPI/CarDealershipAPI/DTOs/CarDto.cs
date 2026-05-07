using System.ComponentModel.DataAnnotations;

namespace CarDealershipAPI.DTOs
{
    public class CarDto
    {
        [Required]
        public string Brand { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(1900, 2100, ErrorMessage = "Enter a valid year")]
        public int Year { get; set; }
    }
}