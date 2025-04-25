using Microsoft.AspNetCore.Mvc;

namespace AirlineManagementSystem.API.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
