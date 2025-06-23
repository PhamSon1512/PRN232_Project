using System.Diagnostics;
using MediAppointment.Client.Models;
using MediAppointment.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediAppointment.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authService;

        public HomeController(ILogger<HomeController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.IsAuthenticated = await _authService.IsAuthenticatedAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
