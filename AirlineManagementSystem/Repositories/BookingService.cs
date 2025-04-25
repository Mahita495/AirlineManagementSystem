using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using AirlineManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using AirlineManagementSystem.Events;


namespace AirlineManagementSystem.Repositories
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private List<BookingDto> bookingDtos;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public BookingService(List<BookingDto> bookingDtos)
        {
            this.bookingDtos = bookingDtos;
        }
        public event EventHandler<BookingEventArgs>? BookingCompleted;

        protected virtual void OnBookingCompleted(BookingEventArgs e)
        {
            BookingCompleted?.Invoke(this, e);
        }

        // Add a new booking to the database
        public async Task AddBookingAsync(BookingDto dto)
        {
            var booking = new Booking
            {
                FlightId = dto.FlightId,
                UserId = dto.UserId,
                From = dto.From,
                To = dto.To,
                DepartureTime = dto.DepartureTime,
                ArrivalTime = dto.ArrivalTime,
                BookingDate = dto.BookingDate,
                Username = dto.Username
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Raise the event after booking is saved
            OnBookingCompleted(new BookingEventArgs
            {
                BookingId = booking.Id,
                FlightId = booking.FlightId,
                UserId = booking.UserId,
                Username = booking.Username,
                BookingDate = booking.BookingDate
            });
        }

        // Get bookings for a specific user
        public async Task<List<BookingDto>> GetBookingsByUserIdAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Flight)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var bookingDtos = bookings.Select(b => new BookingDto
            {
                Id = b.Id,
                FlightId = b.FlightId,
                From = b.From,
                To = b.To,
                DepartureTime = b.DepartureTime,
                ArrivalTime = b.ArrivalTime,
                BookingDate = b.BookingDate,
                Username = b.Username,
                UserId = b.UserId
            }).ToList();

            return bookingDtos;
        }

        // Get a specific booking by its ID
        public async Task<BookingDto> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return null;

            return new BookingDto
            {
                Id = booking.Id,
                FlightId = booking.FlightId,
                From = booking.From,
                To = booking.To,
                DepartureTime = booking.DepartureTime,
                ArrivalTime = booking.ArrivalTime,
                BookingDate = booking.BookingDate,
                Username = booking.Username,
                UserId = booking.UserId
            };
        }
    }
}
