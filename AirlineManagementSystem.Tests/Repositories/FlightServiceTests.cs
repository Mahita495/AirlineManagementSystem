using AirlineManagementSystem.Data;
using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using AirlineManagementSystem.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AirlineManagementSystem.Tests
{
    public class FlightServiceTests
    {
        private readonly AppDbContext _mockContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly FlightService _service;

        public FlightServiceTests()
        {
            // Setup DbContext with in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockContext = new AppDbContext(options);

            _mockMapper = new Mock<IMapper>();
            _mockConfig = new Mock<IConfiguration>();
            _mockCache = new Mock<IMemoryCache>();

            // Setup configuration
            var connectionStringSection = new Mock<IConfigurationSection>();
            connectionStringSection.Setup(x => x.Value).Returns("TestConnectionString");
            _mockConfig.Setup(x => x.GetSection("ConnectionStrings:DefaultConnection"))
                       .Returns(connectionStringSection.Object);

            _service = new FlightService(_mockContext, _mockMapper.Object, _mockConfig.Object, _mockCache.Object);
        }

        [Fact]
        public async Task GetAllAsync_CacheHit_ReturnsCachedFlights()
        {
            // Arrange
            var expectedFlights = new List<FlightDto>
            {
                new FlightDto { Id = 1, FlightNumber = "FL001", Departure = "New York", Destination = "London" },
                new FlightDto { Id = 2, FlightNumber = "FL002", Departure = "Tokyo", Destination = "Paris" }
            };

            // Setup cache to return the expected flights
            object retrievedCache = expectedFlights;
            _mockCache.Setup(x => x.TryGetValue(It.Is<object>(o => o.ToString() == "flightsCache"), out retrievedCache))
                .Returns(true);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Same(expectedFlights, result);
        }

        [Fact]
        public async Task GetAllAsync_CacheMiss_ReturnsFetchedFlights()
        {
            // Arrange
            // Add test data to in-memory database
            var flightEntities = new List<Flight>
            {
                new Flight { Id = 1, FlightNumber = "FL001", Departure = "New York", Destination = "London" },
                new Flight { Id = 2, FlightNumber = "FL002", Departure = "Tokyo", Destination = "Paris" }
            };
            _mockContext.Flights.AddRange(flightEntities);
            await _mockContext.SaveChangesAsync();

            var flightDtos = new List<FlightDto>
            {
                new FlightDto { Id = 1, FlightNumber = "FL001", Departure = "New York", Destination = "London" },
                new FlightDto { Id = 2, FlightNumber = "FL002", Departure = "Tokyo", Destination = "Paris" }
            };

            // Setup cache to miss
            object retrievedCache = null;
            _mockCache.Setup(x => x.TryGetValue(It.Is<object>(o => o.ToString() == "flightsCache"), out retrievedCache))
                .Returns(false);

            // Setup mapper to return the DTOs
            _mockMapper.Setup(m => m.Map<IEnumerable<FlightDto>>(It.IsAny<List<Flight>>()))
                .Returns(flightDtos);

            // Setup cache entry
            var cacheEntryMock = new Mock<ICacheEntry>();
            _mockCache.Setup(x => x.CreateEntry(It.Is<object>(o => o.ToString() == "flightsCache")))
                .Returns(cacheEntryMock.Object);
            cacheEntryMock.Setup(x => x.Dispose());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Equal(flightDtos, result);
        }

        [Fact]
        public async Task GetUsersAsync_CacheHit_ReturnsCachedUsers()
        {
            // Arrange
            var expectedUsers = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "John Doe" },
                new UserDto { Id = 2, Username = "Jane Smith" }
            };

            // Setup cache to return the expected users
            object retrievedCache = expectedUsers;
            _mockCache.Setup(x => x.TryGetValue(It.Is<object>(o => o.ToString() == "usersCache"), out retrievedCache))
                .Returns(true);

            // Act
            var result = await _service.GetUsersAsync();

            // Assert
            Assert.Same(expectedUsers, result);
        }

        [Fact]
        public async Task GetUsersAsync_CacheMiss_ReturnsFetchedUsers()
        {
            // Arrange
            // Add test data to in-memory database with all required properties
            var userEntities = new List<User>
    {
        new User { Id = 1, Username = "John Doe", Email = "john@example.com", Password = "password123", Role = "User" },
        new User { Id = 2, Username = "Jane Smith", Email = "jane@example.com", Password = "password456", Role = "User" }
    };
            _mockContext.Users.AddRange(userEntities);
            await _mockContext.SaveChangesAsync();
            var userDtos = new List<UserDto>
    {
        new UserDto { Id = 1, Username = "John Doe" },
        new UserDto { Id = 2, Username = "Jane Smith" }
    };
            // Setup cache to miss
            object retrievedCache = null;
            _mockCache.Setup(x => x.TryGetValue(It.Is<object>(o => o.ToString() == "usersCache"), out retrievedCache))
                .Returns(false);
            // Setup mapper to return the DTOs
            _mockMapper.Setup(m => m.Map<IEnumerable<UserDto>>(It.IsAny<List<User>>()))
                .Returns(userDtos);
            // Setup cache entry
            var cacheEntryMock = new Mock<ICacheEntry>();
            _mockCache.Setup(x => x.CreateEntry(It.Is<object>(o => o.ToString() == "usersCache")))
                .Returns(cacheEntryMock.Object);
            cacheEntryMock.Setup(x => x.Dispose());
            // Act
            var result = await _service.GetUsersAsync();
            // Assert
            Assert.Equal(userDtos, result);
        }

        [Fact]
        public async Task GetByIdAsync_CacheHit_ReturnsCachedFlight()
        {
            // Arrange
            int flightId = 1;
            string cacheKey = $"flight_{flightId}";
            var expectedFlight = new FlightDto { Id = flightId, FlightNumber = "FL001", Departure = "New York", Destination = "London" };

            // Setup cache to return the expected flight
            object retrievedCache = expectedFlight;
            _mockCache.Setup(x => x.TryGetValue(It.Is<object>(o => o.ToString() == cacheKey), out retrievedCache))
                .Returns(true);

            // Act
            var result = await _service.GetByIdAsync(flightId);

            // Assert
            Assert.Same(expectedFlight, result);
        }

        [Fact]
        public async Task GetByIdAsync_CacheMiss_ReturnsFetchedFlight()
        {
            // Arrange
            int flightId = 1;
            string cacheKey = $"flight_{flightId}";

            // Add test data to in-memory database
            var flightEntity = new Flight { Id = flightId, FlightNumber = "FL001", Departure = "New York", Destination = "London" };
            _mockContext.Flights.Add(flightEntity);
            await _mockContext.SaveChangesAsync();

            var flightDto = new FlightDto { Id = flightId, FlightNumber = "FL001", Departure = "New York", Destination = "London" };

            // Setup cache to miss
            object retrievedCache = null;
            _mockCache.Setup(x => x.TryGetValue(It.Is<object>(o => o.ToString() == cacheKey), out retrievedCache))
                .Returns(false);

            // Setup mapper to return the DTO
            _mockMapper.Setup(m => m.Map<FlightDto>(It.IsAny<Flight>()))
                .Returns(flightDto);

            // Setup cache entry
            var cacheEntryMock = new Mock<ICacheEntry>();
            _mockCache.Setup(x => x.CreateEntry(It.Is<object>(o => o.ToString() == cacheKey)))
                .Returns(cacheEntryMock.Object);
            cacheEntryMock.Setup(x => x.Dispose());

            // Act
            var result = await _service.GetByIdAsync(flightId);

            // Assert
            Assert.Equal(flightDto, result);
        }

        [Fact]
        public async Task AddAsync_ValidFlight_AddsFlightAndClearsCache()
        {
            // Arrange
            var flightDto = new FlightDto { Id = 1, FlightNumber = "FL001", Departure = "New York", Destination = "London" };
            var flightEntity = new Flight { Id = 1, FlightNumber = "FL001", Departure = "New York", Destination = "London" };

            // Setup mapper to return the entity
            _mockMapper.Setup(m => m.Map<Flight>(flightDto))
                .Returns(flightEntity);

            // Act
            await _service.AddAsync(flightDto);

            // Assert
            var savedFlight = await _mockContext.Flights.FindAsync(1);
            Assert.NotNull(savedFlight);
            Assert.Equal("FL001", savedFlight.FlightNumber);

            // Verify cache removal
            _mockCache.Verify(x => x.Remove(It.Is<object>(o => o.ToString() == "flightsCache")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingFlight_UpdatesFlightAndClearsCache()
        {
            // Arrange
            int flightId = 1;

            // Add test flight to in-memory database
            var existingFlight = new Flight { Id = flightId, FlightNumber = "FL001", Departure = "New York", Destination = "London" };
            _mockContext.Flights.Add(existingFlight);
            await _mockContext.SaveChangesAsync();

            var flightDto = new FlightDto { Id = flightId, FlightNumber = "FL001-Updated", Departure = "Boston", Destination = "Paris" };

            // Act
            await _service.UpdateAsync(flightId, flightDto);

            // Assert
            // Refresh from database
            var updatedFlight = await _mockContext.Flights.FindAsync(flightId);
            Assert.Equal("FL001-Updated", updatedFlight.FlightNumber);
            Assert.Equal("Boston", updatedFlight.Departure);
            Assert.Equal("Paris", updatedFlight.Destination);

            // Verify cache removal
            _mockCache.Verify(x => x.Remove(It.Is<object>(o => o.ToString() == "flightsCache")), Times.Once);
            _mockCache.Verify(x => x.Remove(It.Is<object>(o => o.ToString() == $"flight_{flightId}")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingFlight_DoesNotUpdateOrClearCache()
        {
            // Arrange
            int flightId = 999;
            var flightDto = new FlightDto { Id = flightId, FlightNumber = "FL999", Departure = "Tokyo", Destination = "Sydney" };

            // Act
            await _service.UpdateAsync(flightId, flightDto);

            // Assert - No flight should be found with this ID
            var flight = await _mockContext.Flights.FindAsync(flightId);
            Assert.Null(flight);

            // Verify cache was not touched
            _mockCache.Verify(x => x.Remove(It.IsAny<object>()), Times.Never);
        }

        //[Fact]
        //public async Task DeleteAsync_ValidId_DeletesFlightAndClearsCache()
        //{
        //    // Skip this test for now as it uses Dapper with SqlConnection
        //    // which is difficult to mock without additional infrastructure
        //    // Just verify the cache operations
        //    int flightId = 1;
        //    // Act
        //    try
        //    {
        //        await _service.DeleteAsync(flightId);
        //    }
        //    catch
        //    {
        //        // Ignore exceptions related to SQL connections since we're not mocking that part
        //    }
        //    // Assert - only verify cache operations
        //    _mockCache.Verify(x => x.Remove(It.Is<object>(o => o.ToString() == "flightsCache")), Times.Once);
        //    _mockCache.Verify(x => x.Remove(It.Is<object>(o => o.ToString() == $"flight_{flightId}")), Times.Once);
        //}

        [Fact]
        public async Task GetFlightSuggestionsAsync_EmptyTerm_ReturnsEmptyList()
        {
            // Arrange
            string term = "";

            // Act
            var result = await _service.GetFlightSuggestionsAsync(term);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFlightSuggestionsAsync_ValidTerm_ReturnsMatchingFlights()
        {
            // Arrange
            string term = "FL";

            // Add test data to in-memory database
            var flightEntities = new List<Flight>
            {
                new Flight { Id = 1, FlightNumber = "FL001", Departure = "New York", Destination = "London" },
                new Flight { Id = 2, FlightNumber = "FL002", Departure = "Tokyo", Destination = "Paris" },
                new Flight { Id = 3, FlightNumber = "AA123", Departure = "Los Angeles", Destination = "Chicago" }
            };
            _mockContext.Flights.AddRange(flightEntities);
            await _mockContext.SaveChangesAsync();

            // Act
            var result = await _service.GetFlightSuggestionsAsync(term);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains("FL001", result);
            Assert.Contains("FL002", result);
            Assert.DoesNotContain("AA123", result);
        }
    }
}