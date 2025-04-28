using AirlineManagementSystem.Models;

namespace AirlineManagementSystem.DTOs
{
    // FlightDto.cs
    public class FlightDto
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public double Price { get; set; }

        public static implicit operator FlightDto?(Flight? v)
        {
            throw new NotImplementedException();
        }
    }

}
