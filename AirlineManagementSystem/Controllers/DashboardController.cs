using AirlineManagementSystem.Models;
using AirlineManagementSystem.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AirlineManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly IPassengerService _passengerService;

        public DashboardController(IFlightService flightService, IPassengerService passengerService)
        {
            _flightService = flightService;
            _passengerService = passengerService;
        }

        public async Task<IActionResult> Index()
        {
            var flights = await _flightService.GetAllAsync();       // Your existing method
            var passengers = await _passengerService.GetAllPassengersAsync(); // New method

            var viewModel = new DashboardViewModel
            {
                Flights = flights,
                Passengers = passengers
            };

            return View(viewModel);
        }
    }

}
