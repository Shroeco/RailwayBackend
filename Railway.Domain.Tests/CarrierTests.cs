using Railway.Domain.Entities;

namespace Railway.Domain.Tests;

public class CarrierTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesCarrier()
    {
        var id = Guid.NewGuid();

        var carrier = new Carrier(id, "tpe", "TransPennine Express");

        Assert.Equal(id, carrier.Id);
        Assert.Equal("TPE", carrier.Code);
        Assert.Equal("TransPennine Express", carrier.Name);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Carrier(Guid.Empty, "TPE", "TransPennine Express"));
    }

    [Fact]
    public void Constructor_WithEmptyCode_ThrowsException()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new Carrier(id, "", "TransPennine Express"));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsException()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new Carrier(id, "TPE", ""));
    }
}