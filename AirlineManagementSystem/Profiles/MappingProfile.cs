using AirlineManagementSystem.DTOs;
using AirlineManagementSystem.Models;
using AutoMapper;

namespace AirlineManagementSystem.Profiles
{
    // MappingProfile is responsible for setting up AutoMapper configurations
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map Flight to FlightDto and vice versa
            CreateMap<Flight, FlightDto>().ReverseMap();

            // Map User to UserDto and vice versa
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
