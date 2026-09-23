using System.Diagnostics.Contracts;

namespace Railway.Domain.Entities;

public sealed  class Carrier
{
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name {get; private set; }

    public Carrier(Guid id, string code, string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Carrier ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Carrier code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Carrier name cannot be empty.", nameof(name));

        Id = id;
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
    }
}