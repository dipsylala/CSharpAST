using Microsoft.AspNetCore.Mvc;
using MVCApp.Models;
using MVCApp.Services;

namespace MVCApp.Controllers
{
    /// <summary>
    /// Controller for book-related operations
    /// </summary>
    public class BooksController : Controller
    {
        private readonly ILogger<BooksController> _logger;
        private readonly IBookService _bookService;

        public BooksController(ILogger<BooksController> logger, IBookService bookService)
        {
            _logger = logger;
            _bookService = bookService;
        }

        /// <summary>
        /// Display list of books with search and filter capabilities
        /// </summary>
        public async Task<IActionResult> Index(string searchTerm = "", string genre = "", int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Loading books list with search: '{SearchTerm}', genre: '{Genre}', page: {Page}", 
                    searchTerm, genre, page);

                IEnumerable<Book> books;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    books = await _bookService.SearchBooksAsync(searchTerm);
                }
                else if (!string.IsNullOrWhiteSpace(genre))
                {
                    books = await _bookService.GetBooksByGenreAsync(genre);
                }
                else
                {
                    books = await _bookService.GetAllBooksAsync();
                }

                var totalBooks = books.Count();
                var pagedBooks = books.Skip((page - 1) * pageSize).Take(pageSize);

                var viewModel = new BookListViewModel
                {
                    Books = pagedBooks,
                    SearchTerm = searchTerm,
                    SelectedGenre = genre,
                    AvailableGenres = await _bookService.GetAllGenresAsync(),
                    TotalBooks = totalBooks,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books list");
                return View("Error", new ErrorViewModel 
                { 
                    ErrorTitle = "Books List Error",
                    ErrorMessage = "Unable to load the books list. Please try again later." 
                });
            }
        }

        /// <summary>
        /// Display detailed information about a specific book
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Loading book details for ID: {BookId}", id);

                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    _logger.LogWarning("Book not found with ID: {BookId}", id);
                    return NotFound();
                }

                var relatedBooks = await _bookService.GetRelatedBooksAsync(id);

                var viewModel = new BookDetailsViewModel
                {
                    Book = book,
                    RelatedBooks = relatedBooks
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book details for ID: {BookId}", id);
                return View("Error", new ErrorViewModel 
                { 
                    ErrorTitle = "Book Details Error",
                    ErrorMessage = "Unable to load book details. Please try again later." 
                });
            }
        }

        /// <summary>
        /// Search books (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                _logger.LogInformation("AJAX search for term: '{SearchTerm}'", term);

                var books = await _bookService.SearchBooksAsync(term);
                var results = books.Select(b => new 
                { 
                    id = b.Id, 
                    title = b.Title, 
                    author = b.Author,
                    price = b.FormattedPrice
                }).Take(10);

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AJAX search");
                return Json(new { error = "Search failed" });
            }
        }

        /// <summary>
        /// Get books by genre (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByGenre(string genre)
        {
            try
            {
                _logger.LogInformation("Loading books for genre: '{Genre}'", genre);

                var books = await _bookService.GetBooksByGenreAsync(genre);
                var results = books.Select(b => new 
                { 
                    id = b.Id, 
                    title = b.Title, 
                    author = b.Author,
                    price = b.FormattedPrice,
                    coverImageUrl = b.CoverImageUrl
                });

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books by genre");
                return Json(new { error = "Failed to load books" });
            }
        }
    }
}
