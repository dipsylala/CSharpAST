using MVCApp.Models;

namespace MVCApp.Services
{
    /// <summary>
    /// Service interface for book operations
    /// </summary>
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
        Task<IEnumerable<Book>> GetBooksByGenreAsync(string genre);
        Task<IEnumerable<Book>> GetFeaturedBooksAsync();
        Task<IEnumerable<Book>> GetNewReleasesAsync();
        Task<IEnumerable<string>> GetAllGenresAsync();
        Task<IEnumerable<Book>> GetRelatedBooksAsync(int bookId);
    }

    /// <summary>
    /// In-memory implementation of book service
    /// </summary>
    public class BookService : IBookService
    {
        private readonly List<Book> _books = new();

        public BookService()
        {
            SeedSampleData();
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            await Task.Delay(10); // Simulate async operation
            return _books.Where(b => b.IsAvailable).ToList();
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            await Task.Delay(10);
            return _books.FirstOrDefault(b => b.Id == id && b.IsAvailable);
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            await Task.Delay(10);

            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllBooksAsync();

            return _books.Where(b => b.IsAvailable &&
                (b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 b.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<IEnumerable<Book>> GetBooksByGenreAsync(string genre)
        {
            await Task.Delay(10);

            if (string.IsNullOrWhiteSpace(genre))
                return await GetAllBooksAsync();

            return _books.Where(b => b.IsAvailable &&
                b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<IEnumerable<Book>> GetFeaturedBooksAsync()
        {
            await Task.Delay(10);
            return _books.Where(b => b.IsAvailable).Take(6).ToList();
        }

        public async Task<IEnumerable<Book>> GetNewReleasesAsync()
        {
            await Task.Delay(10);
            return _books.Where(b => b.IsAvailable && b.PublishedDate >= DateTime.Now.AddMonths(-6))
                         .OrderByDescending(b => b.PublishedDate)
                         .Take(4)
                         .ToList();
        }

        public async Task<IEnumerable<string>> GetAllGenresAsync()
        {
            await Task.Delay(10);
            return _books.Where(b => b.IsAvailable)
                         .Select(b => b.Genre)
                         .Distinct()
                         .OrderBy(g => g)
                         .ToList();
        }

        public async Task<IEnumerable<Book>> GetRelatedBooksAsync(int bookId)
        {
            await Task.Delay(10);

            var book = await GetBookByIdAsync(bookId);
            if (book == null)
                return Enumerable.Empty<Book>();

            return _books.Where(b => b.IsAvailable && 
                                b.Id != bookId && 
                                b.Genre == book.Genre)
                         .Take(4)
                         .ToList();
        }

        private void SeedSampleData()
        {
            _books.AddRange(new[]
            {
                new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0132350884", 
                          PublishedDate = new DateTime(2008, 8, 1), Genre = "Programming", 
                          Description = "A guide to writing readable, maintainable code.", Price = 45.99m, PageCount = 464 },
                
                new Book { Id = 2, Title = "Design Patterns", Author = "Gang of Four", ISBN = "978-0201633610", 
                          PublishedDate = new DateTime(1994, 10, 31), Genre = "Programming", 
                          Description = "Elements of reusable object-oriented software.", Price = 59.99m, PageCount = 395 },
                
                new Book { Id = 3, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", ISBN = "978-0135957059", 
                          PublishedDate = new DateTime(2019, 9, 13), Genre = "Programming", 
                          Description = "Your journey to mastery, 20th Anniversary Edition.", Price = 49.99m, PageCount = 352 },
                
                new Book { Id = 4, Title = "Dune", Author = "Frank Herbert", ISBN = "978-0441172719", 
                          PublishedDate = new DateTime(1965, 1, 1), Genre = "Science Fiction", 
                          Description = "A science fiction epic of political intrigue and environmental themes.", Price = 16.99m, PageCount = 688 },
                
                new Book { Id = 5, Title = "1984", Author = "George Orwell", ISBN = "978-0451524935", 
                          PublishedDate = new DateTime(1949, 6, 8), Genre = "Dystopian Fiction", 
                          Description = "A dystopian social science fiction novel and cautionary tale.", Price = 13.99m, PageCount = 328 },
                
                new Book { Id = 6, Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "978-0061120084", 
                          PublishedDate = new DateTime(1960, 7, 11), Genre = "Fiction", 
                          Description = "A gripping tale of racial injustice and childhood innocence.", Price = 14.99m, PageCount = 281 },
                
                new Book { Id = 7, Title = "The Art of Computer Programming", Author = "Donald Knuth", ISBN = "978-0201896848", 
                          PublishedDate = new DateTime(1997, 7, 1), Genre = "Programming", 
                          Description = "Fundamental algorithms and data structures.", Price = 199.99m, PageCount = 650 },
                
                new Book { Id = 8, Title = "Introduction to Algorithms", Author = "Thomas H. Cormen", ISBN = "978-0262033848", 
                          PublishedDate = new DateTime(2009, 7, 31), Genre = "Programming", 
                          Description = "Comprehensive textbook on algorithms.", Price = 89.99m, PageCount = 1312 }
            });
        }
    }
}
