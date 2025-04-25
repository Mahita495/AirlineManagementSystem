// Events/BookingEventArgs.cs
namespace AirlineManagementSystem.Events
{
    public class BookingEventArgs : EventArgs
    {
        public int BookingId { get; set; }
        public int FlightId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }
}
