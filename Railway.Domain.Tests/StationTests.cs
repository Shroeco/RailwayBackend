using Railway.Domain.Entities;

namespace Railway.Domain.Tests;

public class StationTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesStation()
    {
        var id = Guid.NewGuid();

        var station = new Station(id, "kgx", "King's Cross");

        Assert.Equal(id, station.Id);
        Assert.Equal("KGX", station.Code);
        Assert.Equal("King's Cross", station.Name);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsException()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new Station(Guid.Empty, "KGX", "King's Cross"));
    }

    [Fact]
    public void Constructor_WithEmptyCode_ThrowsException()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new Station(id, "", "King's Cross"));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsException()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new Station(id, "KGX", ""));
    }
}