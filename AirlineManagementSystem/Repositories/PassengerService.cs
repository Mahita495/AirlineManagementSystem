using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using AutoMapper;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
namespace AirlineManagementSystem.Repositories
{
    public class PassengerService : IPassengerService
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public PassengerService(IConfiguration configuration, IMapper mapper)
        {
            _configuration = configuration;
            _mapper = mapper;
        }
        public async Task<List<PassengerDto>> GetAllPassengersAsync()
        {
            // Use SQL Server connection string since the view is in SQL Server
            var connStr = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);

            // Dapper automatically maps columns to object properties
            var passengers = await conn.QueryAsync<Passenger>("SELECT * FROM vw_Passengers_MySQL");
            return _mapper.Map<List<PassengerDto>>(passengers.ToList());
        }
    }
}