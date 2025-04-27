using AirlineManagementSystem.Controllers;
using AirlineManagementSystem.Data;
using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using AirlineManagementSystem.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace AirlineManagementSystem.Tests.Controllers
{
    public class FlightsControllerTests
    {
        // Mocking dependencies: IFlightService, ILogger, and AppDbContext for FlightsController
        private readonly Mock<IFlightService> _mockService;
        private readonly Mock<ILogger<FlightsController>> _mockLogger;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly FlightsController _controller;

        public FlightsControllerTests()
        {
            // Initializing the mocked services and context
            _mockService = new Mock<IFlightService>();
            _mockLogger = new Mock<ILogger<FlightsController>>();

            // Mock DbContext using an in-memory database option
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestFlightsDb")
                .Options;
            _mockContext = new Mock<AppDbContext>(options);

            // Creating an instance of FlightsController with mocked dependencies
            _controller = new FlightsController(_mockService.Object, _mockLogger.Object, _mockContext.Object);

            // Mocking a user with "Manager" role to simulate authentication context
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "Manager")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user } // Assigning the mocked user to the controller
            };
        }

        // Test for the Index action to ensure it returns a view with flights and pagination
        [Fact]
        public async Task Index_ReturnsViewWithPaginatedFlights()
        {
            // Arrange: Preparing mock data to return from the service
            var flights = new List<FlightDto>
            {
                new FlightDto { FlightNumber = "FL123", Departure = "New York", DepartureTime = DateTime.Now },
                new FlightDto { FlightNumber = "FL124", Departure = "London", DepartureTime = DateTime.Now.AddHours(2) }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(flights);

            // Act: Calling the Index method of the controller with pagination and sorting parameters
            var result = await _controller.Index(null, 1, "Departure", "asc");

            // Assert: Verifying that the returned result is of type ViewResult and contains the correct model
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<FlightDto>>(viewResult.Model);
            Assert.Equal(2, model.Count); // Both flights should be returned with these parameters

            // Verify pagination ViewBag properties
            Assert.Equal(1, viewResult.ViewData["CurrentPage"]);
            Assert.NotNull(viewResult.ViewData["TotalPages"]);
            Assert.Equal("Departure", viewResult.ViewData["SortBy"]);
            Assert.Equal("asc", viewResult.ViewData["SortOrder"]);
        }

        // Test for the Index action with search query to filter the results
        [Fact]
        public async Task Index_WithSearchQuery_FiltersResults()
        {
            // Arrange: Preparing mock data with two flights
            var flights = new List<FlightDto>
            {
                new FlightDto { FlightNumber = "FL123", Departure = "New York", DepartureTime = DateTime.Now },
                new FlightDto { FlightNumber = "FL999", Departure = "London", DepartureTime = DateTime.Now.AddHours(2) }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(flights);

            // Act: Calling the Index method with a search query
            var result = await _controller.Index("123", 1, "Departure", "asc");

            // Assert: Verifying that the returned result filters the flights correctly
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<FlightDto>>(viewResult.Model);
            Assert.Single(model); // Only one flight should match the query
            Assert.Equal("FL123", model.First().FlightNumber); // The filtered flight should be FL123
        }

        // Test for the Index action with sorting by DepartureTime
        [Fact]
        public async Task Index_WithSorting_OrdersResults()
        {
            // Arrange: Preparing mock data with flights having different departure times
            var earlierTime = DateTime.Now;
            var laterTime = DateTime.Now.AddHours(2);
            var flights = new List<FlightDto>
            {
                new FlightDto { FlightNumber = "FL123", Departure = "New York", DepartureTime = laterTime },
                new FlightDto { FlightNumber = "FL999", Departure = "London", DepartureTime = earlierTime }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(flights);

            // Act: Calling the Index method with sorting by departure time descending
            var result = await _controller.Index(null, 1, "departuretime", "desc");

            // Assert: Verifying that the returned result sorts the flights correctly
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<FlightDto>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.Equal("FL123", model.First().FlightNumber); // The later flight should be first
        }

        // Test for the UserDetails action to ensure it filters users correctly
        [Fact]
        public async Task UserDetails_ReturnsFilteredUsers()
        {
            // Arrange: Preparing mock data for users
            var users = new List<UserDto>
            {
                new UserDto { Username = "testuser" },
                new UserDto { Username = "admin" }
            };
            _mockService.Setup(s => s.GetUsersAsync()).ReturnsAsync(users);

            // Act: Calling the UserDetails method with a filter query
            var result = await _controller.UserDetails("test");

            // Assert: Verifying that only one user matches the filter
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserDto>>(viewResult.Model);
            Assert.Single(model); // Only one user should be returned
            Assert.Equal("testuser", model.First().Username); // The filtered user should be testuser
        }

        // Test for the Details action to return flight details when found
        [Fact]
        public async Task Details_ReturnsFlight_WhenFound()
        {
            // Arrange: Preparing mock data for a specific flight
            var flight = new FlightDto { Id = 1, FlightNumber = "FL123" };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(flight);

            // Act: Calling the Details method with the flight ID
            var result = await _controller.Details(1);

            // Assert: Verifying that the returned result contains the correct flight details
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<FlightDto>(viewResult.Model);
            Assert.Equal("FL123", model.FlightNumber); // The flight number should match
        }

        // Test for the Details action to return NotFound when flight is not found
        [Fact]
        public async Task Details_ReturnsNotFound_WhenFlightMissing()
        {
            // Arrange: Mocking a null response when a non-existent flight is requested
            _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((FlightDto)null);

            // Act: Calling the Details method with a non-existent flight ID
            var result = await _controller.Details(999);

            // Assert: Verifying that a NotFoundResult is returned
            Assert.IsType<NotFoundResult>(result);
        }

        // Test for the Create GET action to return the correct view
        [Fact]
        public void Create_Get_ReturnsView()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result); // Verifying that the Create view is returned
        }

        // Test for the Create POST action when the model is invalid
        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithDto()
        {
            // Simulating a model validation error
            _controller.ModelState.AddModelError("FlightNumber", "Required");
            var dto = new FlightDto();

            // Act: Calling the Create method with invalid data
            var result = await _controller.Create(dto);

            // Assert: Verifying that the view is returned with the same DTO
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(dto, viewResult.Model); // The model should be returned as part of the view
        }

        // Test for the Create POST action when the model is valid
        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange: Preparing a valid FlightDto to be created
            var dto = new FlightDto { FlightNumber = "FL321" };

            // Act: Calling the Create method with valid data
            var result = await _controller.Create(dto);

            // Assert: Verifying that a redirect to the Index action occurs
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName); // Redirects to the Index action
        }

        // Test for the Edit GET action to return the correct view with a flight
        [Fact]
        public async Task Edit_Get_ReturnsViewWithFlight()
        {
            // Arrange: Mocking a specific flight for editing
            var dto = new FlightDto { Id = 1, FlightNumber = "FL888" };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(dto);

            // Act: Calling the Edit method with the flight ID
            var result = await _controller.Edit(1);

            // Assert: Verifying that the correct flight data is returned in the view
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(dto, viewResult.Model); // The model in the view should match the flight
        }

        // Test for the Edit POST action to update a flight and redirect
        [Fact]
        public async Task Edit_Post_UpdatesFlight_AndRedirects()
        {
            // Arrange: Preparing a FlightDto for updating
            var dto = new FlightDto { Id = 1, FlightNumber = "FL777" };

            // Act: Calling the Edit method to update the flight
            var result = await _controller.Edit(1, dto);

            // Assert: Verifying that the update service is called once
            _mockService.Verify(s => s.UpdateAsync(1, dto), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName); // Redirects to the Index action after update
        }

        // Test for the Delete action to remove a flight and redirect
        [Fact]
        public async Task Delete_RemovesFlight_AndRedirects()
        {
            // Act: Calling the Delete method for a flight
            var result = await _controller.Delete(1);

            // Assert: Verifying that the Delete service method is called once
            _mockService.Verify(s => s.DeleteAsync(1), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName); // Redirects to the Index action after deletion
        }

        // Test for the Autocomplete action to return flight number suggestions
        [Fact]
        public async Task Autocomplete_ReturnsSuggestions()
        {
            // Arrange: Preparing mock data with flights
            var flights = new List<FlightDto>
            {
                new FlightDto { FlightNumber = "FL123" },
                new FlightDto { FlightNumber = "FL124" },
                new FlightDto { FlightNumber = "AA999" }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(flights);

            // Act: Calling the Autocomplete method with a search term
            var result = await _controller.Autocomplete("FL");

            // Assert: Verifying that the correct suggestions are returned
            var jsonResult = Assert.IsType<JsonResult>(result);
            var suggestions = Assert.IsType<List<string>>(jsonResult.Value);
            Assert.Equal(2, suggestions.Count); // Only FL flights should be returned
            Assert.Contains("FL123", suggestions);
            Assert.Contains("FL124", suggestions);
        }

        
        // Test to verify controller creation with alternative constructor
        [Fact]
        public void AlternativeConstructor_CreatesController()
        {
            // Act: Create controller with alternative constructor
            var controller = new FlightsController(_mockService.Object, _mockLogger.Object);

            // Assert: Verify controller was created successfully
            Assert.NotNull(controller);
        }

        // Test for handling exceptions in Index action
        [Fact]
        public async Task Index_HandlesExceptions_ReturnsErrorView()
        {
            // Arrange: Setup service to throw exception
            _mockService.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("Test exception"));

            // Act: Call Index method which should encounter the exception
            var result = await _controller.Index(null);

            // Assert: Verify error view is returned
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
        }
    }
}