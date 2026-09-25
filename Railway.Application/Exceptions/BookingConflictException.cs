namespace Railway.Application.Exceptions;

public sealed class BookingConflictException : Exception
{
    public BookingConflictException() : base("The booking could not be completed because the journey was modified by another booking.")
    {
        
    }
}