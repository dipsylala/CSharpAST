using Microsoft.AspNetCore.Mvc;
using MVCApp.Models;
using MVCApp.Services;
using System.Diagnostics;

namespace MVCApp.Controllers
{
    /// <summary>
    /// Home controller for the main pages
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookService _bookService;

        public HomeController(ILogger<HomeController> logger, IBookService bookService)
        {
            _logger = logger;
            _bookService = bookService;
        }

        /// <summary>
        /// Home page displaying featured books and new releases
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading home page");

                var viewModel = new HomeViewModel
                {
                    FeaturedBooks = await _bookService.GetFeaturedBooksAsync(),
                    NewReleases = await _bookService.GetNewReleasesAsync(),
                    TotalBooksCount = (await _bookService.GetAllBooksAsync()).Count(),
                    PopularGenres = await _bookService.GetAllGenresAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View("Error", new ErrorViewModel 
                { 
                    ErrorTitle = "Home Page Error",
                    ErrorMessage = "Unable to load the home page. Please try again later." 
                });
            }
        }

        /// <summary>
        /// About page
        /// </summary>
        public IActionResult About()
        {
            _logger.LogInformation("Loading about page");
            ViewData["Message"] = "Welcome to our online library system.";
            return View();
        }

        /// <summary>
        /// Contact page
        /// </summary>
        public IActionResult Contact()
        {
            _logger.LogInformation("Loading contact page");
            ViewData["Message"] = "Get in touch with us.";
            return View();
        }

        /// <summary>
        /// Privacy page
        /// </summary>
        public IActionResult Privacy()
        {
            _logger.LogInformation("Loading privacy page");
            return View();
        }

        /// <summary>
        /// Error page
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            };
            
            _logger.LogWarning("Error page displayed with RequestId: {RequestId}", errorViewModel.RequestId);
            return View(errorViewModel);
        }
    }
}
