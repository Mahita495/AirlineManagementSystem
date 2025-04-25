using System.ComponentModel.DataAnnotations;

namespace AirlineManagementSystem.DTOs{
public class BookingDto
{
    public int Id { get; set; }

    [Required]
    public int FlightId { get; set; }

    public int UserId { get; set; }

    [Required]
    public string From { get; set; }

    [Required]
    public string To { get; set; }

    [Required]
    public DateTime DepartureTime { get; set; }

    [Required]
    public DateTime ArrivalTime { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.Now;

    public string? Username { get; set; }
}
}