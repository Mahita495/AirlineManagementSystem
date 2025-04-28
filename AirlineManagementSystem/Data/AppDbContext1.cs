using AirlineManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineManagementSystem.Data
{
    public class AppDbContext1 : DbContext
    {
        public AppDbContext1(DbContextOptions<AppDbContext1> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        // Other product tables
   }

}