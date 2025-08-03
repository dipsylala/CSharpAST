Imports System
Imports System.Collections.Generic
Imports System.Threading.Tasks

Namespace BookLibrary.Services

    ''' <summary>
    ''' Service for managing book operations in VB.NET
    ''' </summary>
    Public Class BookService
        Private ReadOnly _books As List(Of Book)

        Public Sub New()
            _books = New List(Of Book)()
            InitializeDefaultBooks()
        End Sub

        Public ReadOnly Property BookCount As Integer
            Get
                Return _books.Count
            End Get
        End Property

        Public Function GetAllBooksAsync() As Task(Of List(Of Book))
            Return Task.FromResult(New List(Of Book)(_books))
        End Function

        Public Async Function GetBookByIdAsync(id As Integer) As Task(Of Book)
            Await Task.Delay(10) ' Simulate async operation
            Return _books.FirstOrDefault(Function(b) b.Id = id)
        End Function

        Public Function SearchBooksAsync(searchTerm As String) As Task(Of List(Of Book))
            If String.IsNullOrWhiteSpace(searchTerm) Then
                Return GetAllBooksAsync()
            End If

            Dim filteredBooks = _books.Where(Function(b) 
                b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) OrElse
                b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) OrElse
                b.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList()

            Return Task.FromResult(filteredBooks)
        End Function

        Public Function GetBooksByGenreAsync(genre As String) As Task(Of List(Of Book))
            If String.IsNullOrWhiteSpace(genre) Then
                Return GetAllBooksAsync()
            End If

            Dim filteredBooks = _books.Where(Function(b) 
                b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)
            ).ToList()

            Return Task.FromResult(filteredBooks)
        End Function

        Public Async Function AddBookAsync(book As Book) As Task(Of Boolean)
            Try
                If book Is Nothing Then Return False
                
                book.Id = GetNextId()
                _books.Add(book)
                
                Await LogBookAddedAsync(book)
                Return True
            Catch ex As Exception
                Console.WriteLine($"Error adding book: {ex.Message}")
                Return False
            End Try
        End Function

        Public Function UpdateBookAsync(book As Book) As Task(Of Boolean)
            Dim existingBook = _books.FirstOrDefault(Function(b) b.Id = book.Id)
            If existingBook IsNot Nothing Then
                existingBook.Title = book.Title
                existingBook.Author = book.Author
                existingBook.Genre = book.Genre
                existingBook.Description = book.Description
                existingBook.Price = book.Price
                existingBook.PublishedYear = book.PublishedYear
                existingBook.PageCount = book.PageCount
                existingBook.CoverImageUrl = book.CoverImageUrl
                Return Task.FromResult(True)
            End If
            Return Task.FromResult(False)
        End Function

        Public Function DeleteBookAsync(id As Integer) As Task(Of Boolean)
            Dim bookToRemove = _books.FirstOrDefault(Function(b) b.Id = id)
            If bookToRemove IsNot Nothing Then
                _books.Remove(bookToRemove)
                Return Task.FromResult(True)
            End If
            Return Task.FromResult(False)
        End Function

        Private Function GetNextId() As Integer
            Return If(_books.Count = 0, 1, _books.Max(Function(b) b.Id) + 1)
        End Function

        Private Async Function LogBookAddedAsync(book As Book) As Task
            Await Task.Delay(5) ' Simulate logging operation
            Console.WriteLine($"Book added: {book.Title} by {book.Author}")
        End Function

        Private Sub InitializeDefaultBooks()
            _books.AddRange({
                New Book With {
                    .Id = 1,
                    .Title = "The Great Gatsby",
                    .Author = "F. Scott Fitzgerald",
                    .Genre = "Classic Literature",
                    .Description = "A classic American novel set in the Jazz Age.",
                    .Price = 12.99D,
                    .PublishedYear = 1925,
                    .PageCount = 180,
                    .CoverImageUrl = "/images/gatsby.jpg"
                },
                New Book With {
                    .Id = 2,
                    .Title = "To Kill a Mockingbird",
                    .Author = "Harper Lee",
                    .Genre = "Fiction",
                    .Description = "A gripping tale of racial injustice and loss of innocence.",
                    .Price = 14.99D,
                    .PublishedYear = 1960,
                    .PageCount = 324,
                    .CoverImageUrl = "/images/mockingbird.jpg"
                },
                New Book With {
                    .Id = 3,
                    .Title = "1984",
                    .Author = "George Orwell",
                    .Genre = "Dystopian Fiction",
                    .Description = "A dystopian social science fiction novel.",
                    .Price = 13.99D,
                    .PublishedYear = 1949,
                    .PageCount = 328,
                    .CoverImageUrl = "/images/1984.jpg"
                }
            })
        End Sub
    End Class

End Namespace
