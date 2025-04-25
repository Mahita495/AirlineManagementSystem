using AirlineManagementSystem.DTOs;

namespace AirlineManagementSystem.Repositories
{
    public interface IFlightService
    {
        Task<IEnumerable<FlightDto>> GetAllAsync();          // EF Core
        Task<IEnumerable<UserDto>> GetUsersAsync();        // EF Core
        Task<FlightDto> GetByIdAsync(int id);                // EF Core
        Task AddAsync(FlightDto flight);                     // EF Core
        Task UpdateAsync(int id, FlightDto flight);          // LINQ
        Task DeleteAsync(int id);                            // Dapper

        Task<IEnumerable<string>> GetFlightSuggestionsAsync(string term); //EF Core
      //  Task<IEnumerable<FlightDto>> GetPaginatedAsync(int page, int pageSize); //EF Core
    }

}
