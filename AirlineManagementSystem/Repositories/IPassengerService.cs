using AirlineManagementSystem.DTOs;

namespace AirlineManagementSystem.Repositories
{
    public interface IPassengerService
    {
        Task<List<PassengerDto>> GetAllPassengersAsync();
       
    }
}
