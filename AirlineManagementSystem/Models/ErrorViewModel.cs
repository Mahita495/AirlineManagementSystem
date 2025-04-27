namespace AirlineManagementSystem.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? Message { get; set; } // <-- New property for error message

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
