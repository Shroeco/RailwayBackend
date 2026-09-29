# Carrier Integration Boundary

## Overview

Railway Backend includes an external-carrier boundary designed to support journey data from multiple railway providers without coupling the application to individual carrier implementations.

The current project uses simulated Avanti and LNER providers rather than real external APIs. This keeps the project deterministic and self-contained while demonstrating the architectural boundary that would be used for real carrier integrations.

The design separates the provider contract from its implementations:

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

The Application layer therefore depends on an abstraction rather than directly on either carrier implementation.

## Provider Contract

The carrier boundary is represented by `ICarrierJourneyProvider`.

Each provider exposes a carrier code and an asynchronous journey-search operation conceptually equivalent to:

```csharp
string CarrierCode { get; }

Task<IReadOnlyList<CarrierJourneyResponse>> SearchJourneyAsync(
    Guid originStationId,
    Guid destinationStationId,
    DateTimeOffset departureFrom,
    DateTimeOffset departureTo,
    CancellationToken cancellationToken = default);
```

The contract provides everything required to search a carrier:

- origin station;
- destination station;
- departure-window start;
- departure-window end;
- request cancellation;
- carrier identity.

Implementations return `CarrierJourneyResponse` objects through the shared Application-layer contract.

## Dependency Direction

The interface belongs to the Application layer while concrete providers belong to Infrastructure.

This preserves the dependency direction:

```text
Application
    │
    └── defines carrier abstraction
             ▲
             │
Infrastructure
    └── implements abstraction
```

The Application layer does not need a compile-time dependency on `AvantiJourneyProvider` or `LnerJourneyProvider`.

This means external integration details remain an Infrastructure concern.

## Multiple Carrier Providers

The project contains separate simulated implementations for Avanti and LNER.

Both implement the same `ICarrierJourneyProvider` contract and can therefore be consumed through the abstraction rather than their concrete types.

ASP.NET Core dependency injection can expose the available implementations as:

```text
IEnumerable<ICarrierJourneyProvider>
        │
        ├── AvantiJourneyProvider
        └── LnerJourneyProvider
```

This design allows additional providers to be introduced without requiring the application to be rewritten around a new concrete carrier type.

A future provider would implement the same contract and translate its own external representation into the application's carrier response model.

## Asynchronous Provider Operations

Carrier searches are asynchronous.

The simulated providers use asynchronous delays to represent the latency that would normally occur while communicating with an external network service.

A real provider would typically replace this simulated operation with an HTTP or SDK call to an external carrier.

Keeping the contract asynchronous means the Application layer does not need to change when the simulated providers are replaced with genuine I/O-bound integrations.

## Cancellation and Timeout Boundary

`SearchJourneyAsync` accepts a `CancellationToken`, and provider operations honour cancellation.

Timeout policy is deliberately kept outside the individual provider implementation.

Conceptually:

```text
Caller / Application
        │
        │ controls timeout/cancellation
        ▼
ICarrierJourneyProvider
        │
        │ honours CancellationToken
        ▼
External operation
```

This separates two responsibilities:

- the provider performs the carrier operation;
- the caller decides how long that operation is allowed to run.

A slow provider can therefore be cancelled without embedding application-wide timeout policy into each provider implementation.

This also models how a production integration could combine request cancellation with an externally configured resilience or timeout policy.

## Provider Failures

External systems can fail independently of the Railway Backend.

The simulated integration therefore includes deterministic provider-failure behaviour so that this boundary can be tested without relying on a real external service.

Provider failure is represented through the application-level `CarrierProviderException`.

This prevents the rest of the application from needing to understand low-level implementation failures such as:

```text
HTTP failure
connection failure
DNS failure
third-party SDK exception
```

A future real carrier implementation could translate its provider-specific failure into the same application-level representation.

The Application layer can then reason about a carrier failure without becoming coupled to the technology used to communicate with that carrier.

## External Journey Representation

Carrier search results use `CarrierJourneyResponse` objects rather than exposing a carrier-specific response model directly to the rest of the application.

This creates a translation boundary:

```text
External carrier representation
            │
            ▼
Carrier provider
            │
            ▼
CarrierJourneyResponse
            │
            ▼
Application
```

A real Avanti or LNER API would almost certainly use its own JSON structure, identifiers, naming conventions and data types.

Those details should be translated at the provider boundary rather than allowed to propagate through the Application or Domain layers.

## Testing the Boundary

Carrier-provider behaviour is tested independently from any real external carrier service.

The tests verify important properties of the abstraction, including:

- Avanti can be consumed through `ICarrierJourneyProvider`;
- LNER can be consumed through `ICarrierJourneyProvider`;
- providers return results through the shared carrier response model;
- carrier codes identify the provider;
- simulated provider failures produce the expected application-level failure;
- slow provider operations can be cancelled through the supplied `CancellationToken`.

The cancellation test uses a deliberately slow test provider and a short cancellation timeout.

This verifies that the abstraction supports stopping a slow external operation rather than allowing it to continue indefinitely.

## Why Simulated Providers?

Real railway APIs were intentionally excluded from the scope of this project.

Using simulated providers gives the project a stable