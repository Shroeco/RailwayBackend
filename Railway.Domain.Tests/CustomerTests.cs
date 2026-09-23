using Railway.Domain.Entities;

namespace Railway.Domain.Tests;

public class CustomerTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesCustomer()
    {
        var id = Guid.NewGuid();

        var customer = new Customer(id, "John Smith", "john@example.com");

        Assert.Equal(id, customer.Id);
        Assert.Equal("John Smith", customer.Name);
        Assert.Equal("john@example.com", customer.Email);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Customer(Guid.Empty, "John Smith", "john@example.com"));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Customer(Guid.NewGuid(), "", "john@example.com"));
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Customer(Guid.NewGuid(), "John Smith", ""));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Customer(Guid.NewGuid(), "   ", "john@example.com"));
    }

    [Fact]
    public void Constructor_WithWhitespaceEmail_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Customer(Guid.NewGuid(), "John Smith", "   "));
    }
}