using Railway.Domain.Entities;

namespace Railway.Domain.Tests;

public class FareTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesFare()
    {
        var id = Guid.NewGuid();
        var journeyId = Guid.NewGuid();

        var fare = new Fare(id, journeyId, "Standard", 49.99m);

        Assert.Equal(id, fare.Id);
        Assert.Equal(journeyId, fare.JourneyId);
        Assert.Equal("Standard", fare.Name);
        Assert.Equal(49.99m, fare.Price);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Fare(Guid.Empty, Guid.NewGuid(), "Standard", 49.99m));
    }

    [Fact]
    public void Constructor_WithEmptyJourneyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Fare(Guid.NewGuid(), Guid.Empty, "Standard", 49.99m));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Fare(Guid.NewGuid(), Guid.NewGuid(), "", 49.99m));
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Fare(Guid.NewGuid(), Guid.NewGuid(), "Standard", -1m));
    }

    [Fact]
    public void Constructor_WithZeroPrice_CreatesFare()
    {
        var fare = new Fare(Guid.NewGuid(), Guid.NewGuid(), "Free", 0m);

        Assert.Equal(0m, fare.Price);
    }
}