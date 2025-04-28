using AirlineManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor to initialize the context with DbContextOptions
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet for Flights, mapping to the Flights table in the database
        public DbSet<Flight> Flights { get; set; }

        // DbSet for Users, mapping to the Users table in the database
        public DbSet<User> Users { get; set; }

        // DbSet for Bookings, mapping to the Bookings table in the database
        public DbSet<Booking> Bookings { get; set; }


       





    }
}
