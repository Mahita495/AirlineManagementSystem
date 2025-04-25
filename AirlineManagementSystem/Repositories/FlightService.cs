using AirlineManagementSystem.Data;
using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AirlineManagementSystem.Repositories
{
    public class FlightService : IFlightService
    {
        // EF Core context for accessing the database
        private readonly AppDbContext _context;
        // AutoMapper for converting between models and DTOs
        private readonly IMapper _mapper;
        // Configuration to access connection strings

        private readonly IConfiguration _config; 

        // Constructor to initialize the service with required dependencies
        public FlightService(AppDbContext context, IMapper mapper, IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _config = config;
        }

        public async Task<IEnumerable<FlightDto>> GetAllAsync()
        {
            var flights = await _context.Flights.ToListAsync();  // Fetch all flights using EF Core
            return _mapper.Map<IEnumerable<FlightDto>>(flights);  // Map to DTOs
        }

        // Method to get all users and map them to UserDto objects
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            var users = await _context.Users.ToListAsync();  // Fetch all users using EF Core
            return _mapper.Map<IEnumerable<UserDto>>(users);  // Map to DTOs
        }

        // Method to get a single flight by its ID and map it to FlightDto
        public async Task<FlightDto> GetByIdAsync(int id)
        {
            var flight = await _context.Flights.FindAsync(id);  // Find the flight by ID using EF Core
            return _mapper.Map<FlightDto>(flight);  // Map to DTO
        }

        // Method to add a new flight to the database
        public async Task AddAsync(FlightDto dto)
        {
            var flight = _mapper.Map<Flight>(dto);  // Map DTO to Flight model
            _context.Flights.Add(flight);           // Add the flight to the Flights DbSet
            await _context.SaveChangesAsync();     // Save changes to the database
        }

        // Method to update an existing flight in the database
        public async Task UpdateAsync(int id, FlightDto dto)
        {
            var flight = _context.Flights.FirstOrDefault(f => f.Id == id);  // Find the flight by ID using LINQ
            if (flight != null)
            {
                flight.FlightNumber = dto.FlightNumber;  // Update flight properties
                flight.Departure = dto.Departure;
                flight.Destination = dto.Destination;
                await _context.SaveChangesAsync();  // Save changes to the database
            }
        }

        // Method to delete a flight from the database using Dapper (direct SQL query)
        public async Task DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));  // Open a connection to the database using Dapper
            var query = "DELETE FROM Flights WHERE Id = @Id";  // SQL query to delete a flight
            await conn.ExecuteAsync(query, new { Id = id });  // Execute the SQL query asynchronously
        }

        // Method to get flight suggestions based on a search term using EF Core
        public async Task<IEnumerable<string>> GetFlightSuggestionsAsync(string term)
        {
            var flights = await _context.Flights
                .Where(f => f.FlightNumber.Contains(term))
                .Select(f => f.FlightNumber)
                .Distinct()
                .Take(10)
                .ToListAsync();

            return flights;
        }




    }
}
