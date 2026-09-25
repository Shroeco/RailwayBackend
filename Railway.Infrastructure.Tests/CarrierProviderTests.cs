using Railway.Infrastructure.Carriers.Avanti;
using Railway.Infrastructure.Carriers.Lner;
using Railway.Application.Exceptions;
using Railway.Application.DTO;
using Railway.Application.Services;

namespace Railway.Infrastructure.Tests;

public sealed class CarrierProviderTests
{
    [Fact]
    public async Task SearchJourneyAsync_should_support_cancellation()
    {
        var provider = new AvantiJourneyProvider();

        using var cancellationTokenSource = new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => provider.SearchJourneyAsync(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), cancellationTokenSource.Token));
    }
    [Fact]
    public async Task SearchJourneyAsync_should_throw_carrier_provider_exception_when_provider_fails()
    {
        var provider = new AvantiJourneyProvider(shouldFail: true);

        await Assert.ThrowsAsync<CarrierProviderException>(() => provider.SearchJourneyAsync(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2)));
    }

    [Fact]
    public async Task SearchJourneyAsync_should_support_timeout_cancellation()
    {
        var provider = new SlowCarrierJourneyProvider();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAsync<TaskCanceledException>(() => provider.SearchJourneyAsync( Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Avanti_provider_should_implement_carrier_provider_abstraction()
    {
        ICarrierJourneyProvider provider = new AvantiJourneyProvider();

        var results = await provider.SearchJourneyAsync(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2));

        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("AV", provider.CarrierCode);
    }

    [Fact]
    public async Task Lner_provider_should_implement_carrier_provider_abstraction()
    {
        ICarrierJourneyProvider provider = new LnerJourneyProvider();

        var results = await provider.SearchJourneyAsync(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2));

        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("LNER", provider.CarrierCode);
    }

    private sealed class SlowCarrierJourneyProvider : ICarrierJourneyProvider
    {
        public string CarrierCode => "TEST";

        public async Task<IReadOnlyList<CarrierJourneyResponse>> SearchJourneyAsync(Guid originStationId, Guid destinationStationId, DateTimeOffset departureFrom, DateTimeOffset departureTo, CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            return Array.Empty<CarrierJourneyResponse>();
        }
    }
}