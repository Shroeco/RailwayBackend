# Carrier Integration Boundary

## Purpose

The railway backend is designed to support journey data from multiple carrier providers without coupling the application layer to a specific external service.

The application defines the `ICarrierJourneyProvider` abstraction. Infrastructure contains the concrete provider implementations.

```text
Railway.Application
        │
        │ defines
        ▼
ICarrierJourneyProvider
        ▲
        │ implements
        │
Railway.Infrastructure
    ┌───┴────────────────┐
    │                    │
AvantiJourneyProvider  LnerJourneyProvider
```

## Provider Contract

`ICarrierJourneyProvider` exposes an asynchronous journey-search operation:

```text
SearchJourneyAsync(
    origin,
    destination,
    departure window,
    cancellation token
)
```

Providers return application DTOs rather than domain entities.

This prevents external carrier-specific representations from becoming coupled to the core domain model.

## External Identifiers

Carrier journeys have an `ExternalJourneyId` in addition to the application's own journey identifier.

For example:

```text
Application Journey ID:
7c...abc

External Carrier Journey ID:
AV-1001
```

This allows external systems to maintain their own identifiers without making them the primary identifiers within our database.

## Dependency Injection

Multiple implementations of `ICarrierJourneyProvider` are registered with ASP.NET Core dependency injection.

The application can therefore work with multiple providers through the same abstraction.

```text
IEnumerable<ICarrierJourneyProvider>
        │
        ├── AvantiJourneyProvider
        └── LnerJourneyProvider
```

The application does not need to depend directly on either concrete implementation.

## Asynchronous Operations

Provider calls are asynchronous and accept a `CancellationToken`.

This models the I/O-bound nature of real external services and allows an in-progress provider operation to be cancelled when a request is cancelled or a timeout is reached.

The simulated providers use `Task.Delay` to represent external-service latency.

## Failure Handling

Provider-specific failures are translated into application-level exceptions such as `CarrierProviderException`.

This prevents infrastructure-specific exceptions from leaking into the application layer.

## Timeout and Cancellation Handling

Timeout policy is controlled by the caller rather than being embedded into an individual provider implementation.

Providers honour cancellation through the supplied `CancellationToken`.

This keeps responsibilities separated:

* The provider performs the external operation.
* The caller controls cancellation and timeout policy.
* The application can handle the resulting failure consistently.

## Testing

The provider abstraction is tested independently of a real external carrier service.

Tests verify:

* Providers implement `ICarrierJourneyProvider`.
* Providers return journey DTOs.
* Provider failures are represented consistently.
* Cancellation is honoured.
* Multiple provider implementations can coexist behind the same abstraction.

## Future Real-World Integration

The simulated providers can later be replaced or supplemented with real carrier integrations without changing the core domain model.

A real implementation would typically perform an HTTP/API call and translate the external response into `CarrierJourneyResponse` objects.

The current implementation intentionally avoids real external APIs so that the project remains deterministic, testable and self-contained.
