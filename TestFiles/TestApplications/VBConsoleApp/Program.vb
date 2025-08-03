Imports System
Imports System.Threading.Tasks
Imports BookLibrary.Services
Imports BookLibrary.Models

Module Program
    
    Async Function Main() As Task
        Console.WriteLine("VB.NET Book Library Console Application")
        Console.WriteLine("======================================")
        
        Dim bookService As New BookService()
        
        Try
            ' Display initial books
            Await DisplayAllBooksAsync(bookService)
            
            ' Demonstrate search functionality
            Console.WriteLine(vbCrLf & "Searching for 'great'...")
            Dim searchResults = Await bookService.SearchBooksAsync("great")
            DisplayBooks(searchResults, "Search Results")
            
            ' Demonstrate genre filtering
            Console.WriteLine(vbCrLf & "Filtering by Fiction genre...")
            Dim fictionBooks = Await bookService.GetBooksByGenreAsync("Fiction")
            DisplayBooks(fictionBooks, "Fiction Books")
            
            ' Add a new book
            Console.WriteLine(vbCrLf & "Adding a new book...")
            Dim newBook = New Book With {
                .Title = "Brave New World",
                .Author = "Aldous Huxley",
                .Genre = "Science Fiction",
                .Description = "A dystopian novel exploring themes of technology and society.",
                .Price = 15.99D,
                .PublishedYear = 1932,
                .PageCount = 268,
                .CoverImageUrl = "/images/brave-new-world.jpg"
            }
            
            Dim addResult = Await bookService.AddBookAsync(newBook)
            If addResult Then
                Console.WriteLine($"Successfully added: {newBook.Title}")
                Await DisplayAllBooksAsync(bookService)
            Else
                Console.WriteLine("Failed to add the book.")
            End If
            
            ' Demonstrate book retrieval by ID
            Console.WriteLine(vbCrLf & "Getting book by ID (2)...")
            Dim bookById = Await bookService.GetBookByIdAsync(2)
            If bookById IsNot Nothing Then
                DisplayBookDetails(bookById)
            Else
                Console.WriteLine("Book not found.")
            End If
            
            ' Demonstrate statistics
            Console.WriteLine(vbCrLf & "Library Statistics:")
            Await DisplayLibraryStatisticsAsync(bookService)
            
        Catch ex As Exception
            Console.WriteLine($"An error occurred: {ex.Message}")
        End Try
        
        Console.WriteLine(vbCrLf & "Press any key to exit...")
        Console.ReadKey()
    End Function
    
    Private Async Function DisplayAllBooksAsync(bookService As BookService) As Task
        Console.WriteLine(vbCrLf & "All Books in Library:")
        Console.WriteLine("===================")
        
        Dim allBooks = Await bookService.GetAllBooksAsync()
        DisplayBooks(allBooks, Nothing)
    End Function
    
    Private Sub DisplayBooks(books As List(Of Book), title As String)
        If Not String.IsNullOrEmpty(title) Then
            Console.WriteLine($"{vbCrLf}{title}:")
            Console.WriteLine(New String("="c, title.Length + 1))
        End If
        
        If books.Count = 0 Then
            Console.WriteLine("No books found.")
            Return
        End If
        
        For Each book In books
            Console.WriteLine($"  • {book.GetBookSummary()}")
            Console.WriteLine($"    Price: {book.FormattedPrice} | Pages: {book.PageCount}")
            Console.WriteLine($"    {TruncateDescription(book.Description, 60)}")
            Console.WriteLine()
        Next
    End Sub
    
    Private Sub DisplayBookDetails(book As Book)
        Console.WriteLine("Book Details:")
        Console.WriteLine("=============")
        Console.WriteLine($"Title: {book.Title}")
        Console.WriteLine($"Author: {book.Author}")
        Console.WriteLine($"Genre: {book.Genre}")
        Console.WriteLine($"Published: {book.PublishedYear}")
        Console.WriteLine($"Pages: {book.PageCount}")
        Console.WriteLine($"Price: {book.FormattedPrice}")
        Console.WriteLine($"Classic: {If(book.IsClassic, "Yes", "No")}")
        Console.WriteLine($"Long Book: {If(book.IsLongBook(), "Yes", "No")}")
        Console.WriteLine($"Description: {book.Description}")
    End Sub
    
    Private Async Function DisplayLibraryStatisticsAsync(bookService As BookService) As Task
        Dim allBooks = Await bookService.GetAllBooksAsync()
        
        Console.WriteLine($"Total Books: {bookService.BookCount}")
        Console.WriteLine($"Average Price: {CalculateAveragePrice(allBooks):C}")
        Console.WriteLine($"Average Pages: {CalculateAveragePages(allBooks):F0}")
        Console.WriteLine($"Classic Books: {allBooks.Count(Function(b) b.IsClassic)}")
        Console.WriteLine($"Long Books (>300 pages): {allBooks.Count(Function(b) b.IsLongBook())}")
        
        Dim genreGroups = allBooks.GroupBy(Function(b) b.Genre)
        Console.WriteLine($"Genres: {String.Join(", ", genreGroups.Select(Function(g) $"{g.Key} ({g.Count()})"))}")
    End Function
    
    Private Function CalculateAveragePrice(books As List(Of Book)) As Decimal
        If books.Count = 0 Then Return 0
        Return books.Average(Function(b) b.Price)
    End Function
    
    Private Function CalculateAveragePages(books As List(Of Book)) As Double
        If books.Count = 0 Then Return 0
        Return books.Average(Function(b) CDbl(b.PageCount))
    End Function
    
    Private Function TruncateDescription(description As String, maxLength As Integer) As String
        If String.IsNullOrEmpty(description) Then Return String.Empty
        If description.Length <= maxLength Then Return description
        Return description.Substring(0, maxLength) & "..."
    End Function
    
End Module
