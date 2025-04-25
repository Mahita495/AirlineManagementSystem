using System.ComponentModel.DataAnnotations;

namespace AirlineManagementSystem.Models
{
    public class Flight
    {
        public int Id { get; set; }

        [Required]
        public string FlightNumber { get; set; }

        [Required]
        public string Departure { get; set; }

        [Required]
        public string Destination { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public double Price { get; set; }

        
    }
}

