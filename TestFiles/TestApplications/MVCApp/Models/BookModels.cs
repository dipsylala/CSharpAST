namespace MVCApp.Models
{
    /// <summary>
    /// Book model for the library application
    /// </summary>
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int PageCount { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string CoverImageUrl { get; set; } = "/images/default-book-cover.jpg";

        public string FormattedPrice => Price.ToString("C");
        public string PublishedYear => PublishedDate.Year.ToString();
    }

    /// <summary>
    /// View model for the book list page
    /// </summary>
    public class BookListViewModel
    {
        public IEnumerable<Book> Books { get; set; } = new List<Book>();
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedGenre { get; set; } = string.Empty;
        public IEnumerable<string> AvailableGenres { get; set; } = new List<string>();
        public int TotalBooks { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalBooks / PageSize);
    }

    /// <summary>
    /// View model for book details page
    /// </summary>
    public class BookDetailsViewModel
    {
        public Book Book { get; set; } = new();
        public IEnumerable<Book> RelatedBooks { get; set; } = new List<Book>();
    }

    /// <summary>
    /// View model for the home page
    /// </summary>
    public class HomeViewModel
    {
        public IEnumerable<Book> FeaturedBooks { get; set; } = new List<Book>();
        public IEnumerable<Book> NewReleases { get; set; } = new List<Book>();
        public int TotalBooksCount { get; set; }
        public IEnumerable<string> PopularGenres { get; set; } = new List<string>();
    }

    /// <summary>
    /// Error view model
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorMessage { get; set; } = "An unexpected error occurred.";
        public string ErrorTitle { get; set; } = "Error";
    }
}
