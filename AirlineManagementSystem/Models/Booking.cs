using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlineManagementSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int FlightId { get; set; }

        [ForeignKey("FlightId")]
        public Flight Flight { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public string From { get; set; }

        [Required]
        public string To { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public string Username { get; set; }
    }
}
