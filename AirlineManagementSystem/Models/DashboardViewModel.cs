using AirlineManagementSystem.DTOs;

namespace AirlineManagementSystem.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<FlightDto> Flights { get; set; }
        public IEnumerable<PassengerDto> Passengers { get; set; }
    }

}
