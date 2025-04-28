using AirlineManagementSystem.Data;
using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AirlineManagementSystem.Repositories
{
    public class FlightService : IFlightService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;  // Added cache

        public FlightService(AppDbContext context, IMapper mapper, IConfiguration config, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _config = config;
            _cache = cache;
        }

        /// <summary>
        /// This method returns all the flights data. If at first doesn't get from cache, then fetch from DB
        /// </summary>
        /// <returns>List of flights</returns>

        public async Task<IEnumerable<FlightDto>> GetAllAsync()
        {
            // Try to get from cache first
            if (!_cache.TryGetValue("flightsCache", out IEnumerable<FlightDto> flights))
            {
                // Not in cache, so fetch from database
                var flightEntities = await _context.Flights.ToListAsync();
                flights = _mapper.Map<IEnumerable<FlightDto>>(flightEntities);

                // Set cache options
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));  // Cache expires after 5 minutes of inactivity

                // Save in cache
                _cache.Set("flightsCache", flights, cacheEntryOptions);
            }

            return flights;
        }

        /// <summary>
        /// This method returns all the users data. If at first doesn't get from cache, then fetch from DB
        /// </summary>
        /// <returns>List of users</returns>
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            if (!_cache.TryGetValue("usersCache", out IEnumerable<UserDto> users))
            {
                var userEntities = await _context.Users.ToListAsync();
                users = _mapper.Map<IEnumerable<UserDto>>(userEntities);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set("usersCache", users, cacheEntryOptions);
            }

            return users;
        }

        /// <summary>
        /// This method returns all the flight data by id. If at first doesn't get from cache, then fetch from DB
        /// </summary>
        /// <returns>Data of flight by id</returns>

        public async Task<FlightDto> GetByIdAsync(int id)
        {
            // Simple cache per flight id
            string cacheKey = $"flight_{id}";
            if (!_cache.TryGetValue(cacheKey, out FlightDto flight))
            {
                var flightEntity = await _context.Flights.FindAsync(id);
                flight = _mapper.Map<FlightDto>(flightEntity);

                if (flight != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    _cache.Set(cacheKey, flight, cacheEntryOptions);
                }
            }

            return flight;
        }
        /// <summary>
        /// Add a new flight to the database and clears the cache
        /// </summary>
        /// <param name="dto"></param>
       

        public async Task AddAsync(FlightDto dto)
        {
            var flight = _mapper.Map<Flight>(dto);
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            // Clear the flights cache since data changed
            _cache.Remove("flightsCache");
        }

        /// <summary>
        ///  This method updates the flight data by id and clears the cache
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        

        public async Task UpdateAsync(int id, FlightDto dto)
        {
            var flight = _context.Flights.FirstOrDefault(f => f.Id == id);
            if (flight != null)
            {
                flight.FlightNumber = dto.FlightNumber;
                flight.Departure = dto.Departure;
                flight.Destination = dto.Destination;
                await _context.SaveChangesAsync();

                // Invalidate caches
                _cache.Remove("flightsCache");
                _cache.Remove($"flight_{id}");
            }
        }
        /// <summary>
        /// Deletes a flight by id and clears the cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var query = "DELETE FROM Flights WHERE Id = @Id";
            await conn.ExecuteAsync(query, new { Id = id });

            // Invalidate caches
            _cache.Remove("flightsCache");
            _cache.Remove($"flight_{id}");
        }

        public async Task<IEnumerable<string>> GetFlightSuggestionsAsync(string term)
        {
            if (string.IsNullOrEmpty(term))
                return new List<string>();

            var flights = await _context.Flights
                .Where(f => f.FlightNumber.ToLower().Contains(term.ToLower()))
                .Select(f => f.FlightNumber)
                .Distinct()
                .Take(10)
                .ToListAsync();

            return flights;
        }

    }
}
