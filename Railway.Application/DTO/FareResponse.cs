namespace Railway.Application.DTO;

public sealed class FareResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }
}