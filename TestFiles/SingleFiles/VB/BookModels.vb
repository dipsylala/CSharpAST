Imports System
Imports System.ComponentModel.DataAnnotations

Namespace BookLibrary.Models

    ''' <summary>
    ''' Represents a book in the library system
    ''' </summary>
    Public Class Book
        <Key>
        Public Property Id As Integer

        <Required>
        <StringLength(200)>
        Public Property Title As String = String.Empty

        <Required>
        <StringLength(100)>
        Public Property Author As String = String.Empty

        <StringLength(50)>
        Public Property Genre As String = String.Empty

        <StringLength(1000)>
        Public Property Description As String = String.Empty

        <Range(0, 9999.99)>
        Public Property Price As Decimal

        <Range(1000, 9999)>
        Public Property PublishedYear As Integer

        <Range(1, 10000)>
        Public Property PageCount As Integer

        Public Property CoverImageUrl As String = String.Empty

        Public ReadOnly Property FormattedPrice As String
            Get
                Return Price.ToString("C")
            End Get
        End Property

        Public ReadOnly Property IsClassic As Boolean
            Get
                Return PublishedYear < 1950
            End Get
        End Property

        Public Function GetBookSummary() As String
            Return $"{Title} by {Author} ({PublishedYear}) - {Genre}"
        End Function

        Public Function IsLongBook() As Boolean
            Return PageCount > 300
        End Function

        Public Shared Function CreateSampleBook() As Book
            Return New Book With {
                .Title = "Sample Book",
                .Author = "Sample Author",
                .Genre = "Fiction",
                .Description = "A sample book for testing purposes.",
                .Price = 9.99D,
                .PublishedYear = 2023,
                .PageCount = 250,
                .CoverImageUrl = "/images/sample.jpg"
            }
        End Function
    End Class

    ''' <summary>
    ''' View model for book listing pages
    ''' </summary>
    Public Class BookListViewModel
        Public Property Books As List(Of Book) = New List(Of Book)()
        Public Property SearchTerm As String = String.Empty
        Public Property SelectedGenre As String = String.Empty
        Public Property AvailableGenres As List(Of String) = New List(Of String)()
        Public Property CurrentPage As Integer = 1
        Public Property TotalPages As Integer = 1
        Public Property PageSize As Integer = 12

        Public ReadOnly Property TotalBooks As Integer
            Get
                Return Books.Count
            End Get
        End Property

        Public ReadOnly Property HasPreviousPage As Boolean
            Get
                Return CurrentPage > 1
            End Get
        End Property

        Public ReadOnly Property HasNextPage As Boolean
            Get
                Return CurrentPage < TotalPages
            End Get
        End Property

        Public Function GetPageNumbers() As List(Of Integer)
            Dim startPage = Math.Max(1, CurrentPage - 2)
            Dim endPage = Math.Min(TotalPages, CurrentPage + 2)
            
            Dim pages As New List(Of Integer)()
            For i = startPage To endPage
                pages.Add(i)
            Next
            
            Return pages
        End Function
    End Class

    ''' <summary>
    ''' Enum for book genres
    ''' </summary>
    Public Enum BookGenre
        Fiction
        NonFiction
        Mystery
        Romance
        SciFi
        Fantasy
        Biography
        History
        SelfHelp
        Technology
    End Enum

    ''' <summary>
    ''' Interface for book operations
    ''' </summary>
    Public Interface IBookRepository
        Function GetAllBooksAsync() As Task(Of List(Of Book))
        Function GetBookByIdAsync(id As Integer) As Task(Of Book)
        Function AddBookAsync(book As Book) As Task(Of Integer)
        Function UpdateBookAsync(book As Book) As Task(Of Boolean)
        Function DeleteBookAsync(id As Integer) As Task(Of Boolean)
        Function SearchBooksAsync(searchTerm As String) As Task(Of List(Of Book))
    End Interface

End Namespace
