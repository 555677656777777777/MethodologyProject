namespace CarDealershipAPI.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; }       // e.g. Toyota
        public string Model { get; set; }       // e.g. Camry
        public decimal Price { get; set; }      // e.g. 15000.00
        public int Year { get; set; }           // e.g. 2022
        public bool IsAvailable { get; set; }   // false = already sold
    }
}