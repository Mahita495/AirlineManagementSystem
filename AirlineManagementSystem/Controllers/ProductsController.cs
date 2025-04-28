using AirlineManagementSystem.Data;
using AirlineManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineManagementSystem.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _airlineDbContext;
        private readonly AppDbContext1 _productDbContext;

        public ProductsController(AppDbContext airlineDbContext, AppDbContext1 productDbContext)
        {
            _airlineDbContext = airlineDbContext;
            _productDbContext = productDbContext;
        }

        public async Task<IActionResult> Dashboard()
        {
            var flights = await _airlineDbContext.Flights.ToListAsync();
            var products = await _productDbContext.Products.ToListAsync();

            var viewModel = new ProductsAndFlights
            {
                Flights = flights,
                Products = products
            };

            return View(viewModel);
        }
    }
}
