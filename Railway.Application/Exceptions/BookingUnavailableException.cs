namespace Railway.Application.Exceptions;

public sealed class BookingUnavailableException : Exception
{
    public BookingUnavailableException() : base("The journey has no available seats.")
    {
        
    }
}