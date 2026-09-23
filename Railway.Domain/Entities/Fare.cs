namespace Railway.Domain.Entities;

public sealed class Fare
{
    public Guid Id { get; private set; }
    public Guid JourneyId { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public Fare(Guid id, Guid journeyId, string name, decimal price)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Fare ID cannot be empty", nameof(id));
        
        if (journeyId == Guid.Empty)
            throw new ArgumentException("Jounery ID cannot be empty", nameof(journeyId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Fare name cannot be empty", nameof(name));

        if (price < 0)
            throw new ArgumentException("Fare price cannot be negative", nameof(price));

        Id = id;
        JourneyId = journeyId;
        Name = name.Trim();
        Price = price;
    }
}