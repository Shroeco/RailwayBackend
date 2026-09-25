namespace Railway.Application.Exceptions;

public sealed class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resourceName) : base($"{resourceName} was not found.")
    {
        
    }
}