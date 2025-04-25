using AirlineManagementSystem.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirlineManagementSystem.Repositories
{
    public interface IBookingService
    {
        Task AddBookingAsync(BookingDto dto);
        Task<List<BookingDto>> GetBookingsByUserIdAsync(int userId);
        Task<BookingDto> GetBookingByIdAsync(int bookingId);
    }
}
