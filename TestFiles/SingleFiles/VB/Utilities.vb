Imports System
Imports System.Threading.Tasks

Module UtilityModule

    ''' <summary>
    ''' Utility functions for string operations
    ''' </summary>
    Public Class StringUtilities
        Public Shared Function TruncateString(input As String, maxLength As Integer) As String
            If String.IsNullOrEmpty(input) Then Return String.Empty
            If input.Length <= maxLength Then Return input
            Return input.Substring(0, maxLength) & "..."
        End Function

        Public Shared Function ToTitleCase(input As String) As String
            If String.IsNullOrEmpty(input) Then Return String.Empty
            
            Dim words = input.Split(" "c)
            For i = 0 To words.Length - 1
                If words(i).Length > 0 Then
                    words(i) = Char.ToUpper(words(i)(0)) & words(i).Substring(1).ToLower()
                End If
            Next
            
            Return String.Join(" ", words)
        End Function

        Public Shared Function RemoveSpecialCharacters(input As String) As String
            If String.IsNullOrEmpty(input) Then Return String.Empty
            Return System.Text.RegularExpressions.Regex.Replace(input, "[^a-zA-Z0-9\s]", "")
        End Function
    End Class

    ''' <summary>
    ''' Mathematical utility functions
    ''' </summary>
    Public Class MathUtilities
        Public Shared Function CalculateAverage(values As Double()) As Double
            If values Is Nothing OrElse values.Length = 0 Then Return 0
            Return values.Sum() / values.Length
        End Function

        Public Shared Function FindMaximum(values As Integer()) As Integer?
            If values Is Nothing OrElse values.Length = 0 Then Return Nothing
            Return values.Max()
        End Function

        Public Shared Function IsPrime(number As Integer) As Boolean
            If number <= 1 Then Return False
            If number = 2 Then Return True
            If number Mod 2 = 0 Then Return False
            
            For i = 3 To Math.Sqrt(number) Step 2
                If number Mod i = 0 Then Return False
            Next
            
            Return True
        End Function

        Public Shared Function Factorial(n As Integer) As Long
            If n < 0 Then Throw New ArgumentException("Factorial is not defined for negative numbers")
            If n = 0 OrElse n = 1 Then Return 1
            
            Dim result As Long = 1
            For i = 2 To n
                result *= i
            Next
            
            Return result
        End Function
    End Class

    ''' <summary>
    ''' Asynchronous utility functions
    ''' </summary>
    Public Class AsyncUtilities
        Public Shared Async Function DelayedExecutionAsync(Of T)(func As Func(Of T), delayMs As Integer) As Task(Of T)
            Await Task.Delay(delayMs)
            Return func()
        End Function

        Public Shared Async Function RetryAsync(Of T)(func As Func(Of Task(Of T)), maxRetries As Integer, delayMs As Integer) As Task(Of T)
            Dim attempts = 0
            
            While attempts <= maxRetries
                Try
                    Return Await func()
                Catch ex As Exception When attempts < maxRetries
                    attempts += 1
                    Await Task.Delay(delayMs)
                End Try
            End While
            
            Throw New InvalidOperationException($"Operation failed after {maxRetries} retries")
        End Function

        Public Shared Async Function ProcessBatchAsync(Of T, TResult)(
            items As IEnumerable(Of T),
            processor As Func(Of T, Task(Of TResult)),
            batchSize As Integer
        ) As Task(Of List(Of TResult))
            
            Dim results As New List(Of TResult)()
            Dim batch As New List(Of T)()
            
            For Each item In items
                batch.Add(item)
                
                If batch.Count = batchSize Then
                    Dim batchTasks = batch.Select(Function(b) processor(b))
                    Dim batchResults = Await Task.WhenAll(batchTasks)
                    results.AddRange(batchResults)
                    batch.Clear()
                End If
            Next
            
            ' Process remaining items
            If batch.Count > 0 Then
                Dim finalTasks = batch.Select(Function(b) processor(b))
                Dim finalResults = Await Task.WhenAll(finalTasks)
                results.AddRange(finalResults)
            End If
            
            Return results
        End Function
    End Class

    ''' <summary>
    ''' Extension methods for common operations
    ''' </summary>
    <System.Runtime.CompilerServices.Extension>
    Public Module Extensions
        <System.Runtime.CompilerServices.Extension>
        Public Function IsNullOrEmpty(value As String) As Boolean
            Return String.IsNullOrEmpty(value)
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function ToSafeString(obj As Object) As String
            Return If(obj?.ToString(), String.Empty)
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function TakeRandomly(Of T)(source As IEnumerable(Of T), count As Integer) As IEnumerable(Of T)
            Dim random As New Random()
            Return source.OrderBy(Function(x) random.Next()).Take(count)
        End Function
    End Module

End Module
