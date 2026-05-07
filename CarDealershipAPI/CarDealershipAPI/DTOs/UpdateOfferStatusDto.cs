using System.ComponentModel.DataAnnotations;

namespace CarDealershipAPI.DTOs
{
    public class UpdateOfferStatusDto
    {
        [Required]
        [RegularExpression("Accepted|Rejected",
            ErrorMessage = "Status must be 'Accepted' or 'Rejected'")]
        public string Status { get; set; }
    }
}