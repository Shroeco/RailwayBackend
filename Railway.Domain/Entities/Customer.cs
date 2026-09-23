namespace Railway.Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    public Customer(Guid id, string name, string email)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Customer email cannot be empty", nameof(email));

        Id = id;
        Name = name.Trim();
        Email = email.Trim();
    }
}