using AirlineManagementSystem.Data;
using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using AirlineManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirlineManagementSystem.Controllers
{
    //[Authorize(Roles = "Manager,User")] 
    [Route("[controller]")]
    public class FlightsController : Controller
    {
        // Flight service interface
        private readonly IFlightService _service;
       
        // Logger for logging information and errors

        private readonly ILogger<FlightsController> _logger;
        private IFlightService object1;
        private ILogger<FlightsController> object2;

        ///<summary>
        /// Constructor to inject service and logger.
        ///</summary>
        public FlightsController(IFlightService service, ILogger<FlightsController> logger)
        {
            _service = service;
            _logger = logger;
            
        }


        ///<summary>
        /// Display list of flights with optional search, sorting, and pagination.
        ///</summary>
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index(string search, int page = 1, string sortBy = "Departure", string sortOrder = "asc")
        {
            try
            {
                _logger.LogInformation("GET Index page requested with search query: {SearchQuery}, page number: {Page}, sort by: {SortBy}, sort order: {SortOrder}.", search, page, sortBy, sortOrder);

                // Define page size
                int pageSize = 5;

                var flights = await _service.GetAllAsync();

                // Filter flights based on search query
                if (!string.IsNullOrEmpty(search))
                {
                    flights = flights.Where(f => f.FlightNumber.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                    _logger.LogInformation("Filtered flights by FlightNumber: {SearchQuery}.", search);
                }

                // Apply sorting
                switch (sortBy.ToLower())
                { 
                    case "departure":
                        flights = sortOrder == "asc" ? flights.OrderBy(f => f.Departure).ToList() : flights.OrderByDescending(f => f.Departure).ToList();
                        break;
                    case "departuretime":
                        flights = sortOrder == "asc" ? flights.OrderBy(f => f.DepartureTime).ToList() : flights.OrderByDescending(f => f.DepartureTime).ToList();
                        break;
                    default:
                        break;
                }

                // Paginate results
                var paginatedFlights = flights.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                int totalFlights = flights.Count();
                int totalPages = (int)Math.Ceiling((double)totalFlights / pageSize);

                // Set pagination-related ViewBag properties
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.SearchQuery = search;
                ViewBag.SortBy = sortBy;
                ViewBag.SortOrder = sortOrder;

                // Pass role and username to view
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                ViewBag.Role = userRole;
                var username = User.Identity.Name;
                ViewBag.Username = username;

                return View(paginatedFlights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching flights.");
                return View("Error", new { Message = "An error occurred while retrieving flights." });
            }
        }

        ///<summary>
        /// Display user details with optional search.
        ///</summary>
        [Route("UserDetails")]
        public async Task<IActionResult> UserDetails(string search)
        {
            try
            {
                _logger.LogInformation("GET UserDetails page requested with search query: {SearchQuery}.", search);
                var users = await _service.GetUsersAsync(); // Get all users

                if (!string.IsNullOrEmpty(search))
                {
                    // Filter by username
                    users = users.Where(f => f.Username.Contains(search)).ToList();
                    _logger.LogInformation("Filtered users by Username: {SearchQuery}.", search);
                }

                return View(users); // Return view with user list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
                return View("Error", new { Message = "An error occurred while retrieving users." });
            }
        }

        ///<summary>
        /// Display flight details by ID.
        ///</summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("GET Details page requested for FlightId: {FlightId}.", id);
                var flight = await _service.GetByIdAsync(id); // Get flight by ID

                if (flight == null)
                {
                    _logger.LogWarning("Flight with ID {FlightId} not found.", id);
                    return NotFound(); // Return 404 if not found
                }

                return View(flight); // Return view with flight details
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching flight details.");
                return View("Error", new { Message = "An error occurred while retrieving the flight details." });
            }
        }

        ///<summary>
        /// Show create flight form (Only for Managers).
        ///</summary>
        [Authorize(Roles = "Manager")]
        [Route("Create")]
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("GET Create flight page requested.");
                return View(); // Return empty create view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while requesting create flight page.");
                return View("Error", new { Message = "An error occurred while loading the create flight page." });
            }
        }

        ///<summary>
        /// Handle POST request to create new flight (Only for Managers).
        ///</summary>
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(FlightDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for creating flight: {FlightDto}.", dto);
                    return View(dto); // Return with validation errors
                }

                _logger.LogInformation("Creating new flight: {FlightNumber}.", dto.FlightNumber);
                await _service.AddAsync(dto); // Add new flight
                _logger.LogInformation("New flight created: {FlightNumber}.", dto.FlightNumber);

                return RedirectToAction("Index"); // Redirect to flight list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating flight.");
                return View("Error", new { Message = "An error occurred while creating the flight." });
            }
        }

        ///<summary>
        /// Show edit flight form (Only for Managers).
        ///</summary>
        [Authorize(Roles = "Manager")]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("GET Edit page requested for FlightId: {FlightId}.", id);
                var flight = await _service.GetByIdAsync(id); // Get flight to edit

                if (flight == null)
                {
                    _logger.LogWarning("Flight with ID {FlightId} not found for editing.", id);
                    return NotFound(); // Return 404 if not found
                }

                return View(flight); // Return view with flight data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while requesting the edit flight page.");
                return View("Error", new { Message = "An error occurred while loading the edit flight page." });
            }
        }

        ///<summary>
        /// Handle POST request to edit flight (Only for Managers).
        ///</summary>
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int id, FlightDto dto)
        {
            try
            {
                _logger.LogInformation("POST Edit flight requested for FlightId: {FlightId}.", id);
                await _service.UpdateAsync(id, dto); // Update flight
                _logger.LogInformation("Flight updated with ID {FlightId}.", id);

                return RedirectToAction("Index"); // Redirect to flight list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the flight.");
                return View("Error", new { Message = "An error occurred while updating the flight." });
            }
        }

        ///<summary>
        /// Delete flight by ID.
        ///</summary>
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("DELETE flight requested for FlightId: {FlightId}.", id);
                await _service.DeleteAsync(id); // Delete flight
                _logger.LogInformation("Flight with ID {FlightId} deleted.", id);

                return RedirectToAction("Index"); // Redirect to flight list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the flight.");
                return View("Error", new { Message = "An error occurred while deleting the flight." });
            }
        }

        ///<summary>
        /// Autocomplete flight number based on search term.
        ///</summary>

        [HttpGet]
        [Route("Autocomplete")]
        public async Task<IActionResult> Autocomplete(string term)
        {
            var suggestions = await _service.GetFlightSuggestionsAsync(term);
            return Json(suggestions);
        }





    }
}
