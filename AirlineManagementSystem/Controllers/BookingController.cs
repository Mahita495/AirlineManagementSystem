using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Events;
using AirlineManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AirlineManagementSystem.Controllers
{
    [Authorize(Roles = "User,Manager")]
    [Route("[controller]")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IFlightService _flightService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IBookingService bookingService,
            IFlightService flightService,
            IUserRepository userRepository,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _flightService = flightService;
            _userRepository = userRepository;
            _logger = logger;

            // Subscribe to booking completed event - test git
            if (_bookingService is BookingService concreteService)
            {
                concreteService.BookingCompleted += HandleBookingCompleted;
            }
        }

        ///<summary>
        /// Handles the booking completed event and logs the event details.
        /// This method is triggered whenever a booking is completed.
        ///</summary>
        private void HandleBookingCompleted(object? sender, BookingEventArgs e)
        {
            _logger.LogInformation("📢 Booking event triggered: User {Username} (ID: {UserId}) booked Flight {FlightId} at {Time}.",
                e.Username, e.UserId, e.FlightId, e.BookingDate);
        }

        ///<summary>
        /// Loads the booking form for a specific flight based on the flight ID.
        /// This method displays the booking form where users can select a flight to book.
        ///</summary>
        ///<param name="flightId">The ID of the flight to be booked.</param>
        ///<returns>Returns the booking form view with the flight details populated.</returns>
        [HttpGet("book")]
        public async Task<IActionResult> Book(int flightId)
        {
            try
            {
                // Fetch flight details by ID
                var flight = await _flightService.GetByIdAsync(flightId);
                if (flight == null)
                {
                    _logger.LogWarning("Flight with ID {FlightId} not found for booking.", flightId);
                    return NotFound();
                }

                // Populate the booking DTO with flight details
                var bookingDto = new BookingDto
                {
                    FlightId = flight.Id,
                    From = flight.Departure,
                    To = flight.Destination,
                    DepartureTime = flight.DepartureTime,
                    ArrivalTime = flight.ArrivalTime
                };

                _logger.LogInformation("Loaded booking form for flight ID {FlightId}.", flightId);
                return View(bookingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking form for flight ID {FlightId}.", flightId);
                return View("Error", new { Message = "An error occurred while loading the booking form." });
            }
        }

        ///<summary>
        /// Handles the booking form submission and creates a new booking.
        /// This method is triggered when the user submits the booking form.
        /// It validates the form and processes the booking request.
        ///</summary>
        ///<param name="dto">The booking data transfer object containing the booking details.</param>
        ///<returns>Redirects to the show bookings page if successful, or returns the booking form view if there are validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost("book")]
        public async Task<IActionResult> Book(BookingDto dto)
        {
            try
            {
                // Get username from User.Identity
                string username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Unauthorized booking attempt - no username found.");
                    return RedirectToAction("Login", "Account");
                }

                dto.Username = username;

                // Look up user by username using the UserRepository
                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("User not found for username: {Username}", username);
                    return RedirectToAction("Login", "Account");
                }

                // Set the user ID from the retrieved user
                dto.UserId = user.Id;

                // Check if the model state is valid
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid booking form submission.");
                    foreach (var modelStateEntry in ModelState.Values)
                    {
                        foreach (var error in modelStateEntry.Errors)
                        {
                            _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                        }
                    }
                    return View(dto);
                }

                // Refetch flight details to ensure they're correct
                var flight = await _flightService.GetByIdAsync(dto.FlightId);
                if (flight == null)
                {
                    ModelState.AddModelError("", "Flight not found.");
                    return View(dto);
                }

                // Set the flight details from the database
                dto.From = flight.Departure;
                dto.To = flight.Destination;
                dto.DepartureTime = flight.DepartureTime;
                dto.ArrivalTime = flight.ArrivalTime;

                // Set the booking date to the current time
                dto.BookingDate = DateTime.Now;

                // Call the booking service to add the booking
                await _bookingService.AddBookingAsync(dto);

                _logger.LogInformation("Booking created by user {Username} (ID: {UserId}) for flight {FlightId}.",
                    dto.Username, dto.UserId, dto.FlightId);
                return RedirectToAction("ShowBookings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking.");
                return View("Error", new { Message = "An error occurred while booking the flight." });
            }
        }

        ///<summary>
        /// Retrieves and displays all bookings made by the logged-in user.
        /// This method shows a list of bookings associated with the user's account.
        ///</summary>
        ///<returns>Returns the view with the list of bookings for the user.</returns>
        [HttpGet("showBookings")]
        public async Task<IActionResult> ShowBookings()
        {
            try
            {
                // Get username from User.Identity
                string username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Unauthorized attempt to view bookings - no username found.");
                    return RedirectToAction("Login", "Auth");
                }

                // Look up user by username using the UserRepository
                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("User not found for username: {Username}", username);
                    return RedirectToAction("Login", "Auth");
                }

                // Use the user ID from the retrieved user
                var userId = user.Id;

                // Retrieve all bookings for the user
                var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
                return View(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user bookings.");
                return View("Error", new { Message = "An error occurred while fetching bookings." });
            }
        }
    }
}
