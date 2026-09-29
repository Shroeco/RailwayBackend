# Railway Backend — Portfolio Guide

## Project Summary

Railway Backend is a production-style railway journey search and booking REST API built with C# and .NET 8.

The project was designed to demonstrate backend engineering beyond basic CRUD operations. It includes layered architecture, PostgreSQL persistence, transactional booking, optimistic concurrency protection, simulated multi-carrier integration, automated testing, containerisation, continuous integration, production deployment, structured error handling, correlation IDs, and health monitoring.

The application is deployed to Render and backed by a managed PostgreSQL database.

## Technology Stack

- C#
- .NET 8
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- xUnit
- Docker
- Docker Compose
- GitHub Actions
- Swagger / OpenAPI
- Render
- Git / GitHub

## Strongest Technical Achievements

### Concurrency-Safe Booking

The strongest technical feature is protection against overselling the final available seat.

Journey updates use PostgreSQL optimistic concurrency through the `xmin` row version. Booking creation updates journey availability and persists the booking transactionally.

Concurrency is tested both at the persistence layer and through the complete HTTP application.

The HTTP test creates a journey with exactly one remaining seat and sends two booking requests concurrently from separate clients.

The expected result is:

```text
Request A ──► 201 Created
Request B ──► 409 Conflict

Bookings persisted: 1
Available seats: 0
```

The test also verifies that the seat count cannot become negative and that the persisted booking belongs to the successful request.

This demonstrates database concurrency, transactions, race-condition testing and correct HTTP conflict handling.

### Layered Architecture

The solution separates responsibilities across four main projects:

```text
Railway.Api
Railway.Application
Railway.Domain
Railway.Infrastructure
```

The Domain contains core business behaviour, Application coordinates use cases, Infrastructure implements persistence and external-service boundaries, and API handles HTTP concerns.

Dependencies are directed toward the core rather than allowing infrastructure concerns to spread through the application.

### PostgreSQL Persistence

Entity Framework Core is backed by PostgreSQL in development, integration testing, Docker and production.

Database-dependent tests deliberately use PostgreSQL rather than an EF Core in-memory replacement so that transactions, constraints and optimistic concurrency are exercised against the real database technology.

### External Carrier Boundary

Journey-provider integration is represented by `ICarrierJourneyProvider`.

Simulated Avanti and LNER providers implement the same abstraction and support asynchronous operations, cancellation and provider-failure handling.

The boundary demonstrates how real carrier integrations could later replace the simulated providers without coupling the Application or Domain layers to individual external services.

### Automated Testing

The project contains **85 automated tests** across:

- Domain tests
- Application tests
- Infrastructure/PostgreSQL tests
- API integration tests

Important scenarios include transaction rollback, database isolation, concurrent booking, HTTP conflict handling, correlation-ID propagation and safe unexpected-error responses.

### Production Observability

Every request can be associated with an `X-Correlation-ID`.

Clients may supply an ID or the API generates one. Correlation IDs are propagated through response headers, request logs and handled `ProblemDetails` responses.

Expected application failures are logged separately from unexpected server failures, while unexpected exception details remain hidden from API consumers.

The application exposes:

```text
/health
/health/live
/health/ready
```

This separates overall application health, process liveness and database-backed readiness.

### Docker and CI/CD

The API uses a multi-stage Docker build and runs as a non-root user.

Docker Compose provides a reproducible local API/PostgreSQL environment with health-aware startup and persistent database storage.

GitHub Actions automatically:

1. restores dependencies;
2. builds the solution with warnings treated as errors;
3. runs all four test projects against PostgreSQL;
4. builds the production Docker image.

The final verified baseline is:

```text
85 tests passed
0 tests failed
0 build warnings
0 build errors
```

### Production Deployment

The application is deployed to Render using the same Dockerfile maintained in the repository.

Production uses a managed PostgreSQL database, environment-based connection configuration, automatic EF Core migrations and operational health endpoints.

The deployed system has been verified through the complete booking lifecycle:

```text
Search / retrieve journey
        ↓
Create booking
        ↓
Seat availability decreases
        ↓
Retrieve persisted booking
        ↓
Cancel booking
        ↓
Retrieve cancelled booking
```

## CV-Ready Project Entry

### Railway Backend — C#/.NET Backend Engineering Project

Built and deployed a production-style railway journey search and booking REST API using C#, .NET 8, ASP.NET Core, Entity Framework Core and PostgreSQL.

- Designed a layered architecture separating API, application, domain and infrastructure concerns.
- Implemented transactional booking with PostgreSQL/EF Core optimistic concurrency to prevent overselling under simultaneous requests.
- Built an HTTP-level concurrency test proving that two customers competing for one remaining seat result in exactly one successful booking.
- Created a replaceable multi-carrier integration boundary with asynchronous Avanti and LNER provider simulations, cancellation and failure handling.
- Developed 85 automated domain, application, PostgreSQL integration and API tests.
- Containerised the application with Docker and PostgreSQL and implemented GitHub Actions CI for warning-free builds, automated tests and Docker image validation.
- Added production observability using correlation IDs, structured request/error logging, `ProblemDetails`, and health/liveness/readiness endpoints.
- Deployed the containerised API and PostgreSQL database to Render and verified the complete booking lifecycle in production.

## Short CV Version

For a CV with limited space:

**Railway Backend — C#/.NET 8, ASP.NET Core, EF Core, PostgreSQL, Docker**

Developed and deployed a production-style railway booking REST API using layered architecture and PostgreSQL. Implemented transactional seat reservation with optimistic concurrency protection and an HTTP-level race-condition test proving that simultaneous requests cannot oversell the final seat. Built 85 automated tests across domain, application, infrastructure and API layers, multi-carrier abstractions, Docker deployment, GitHub Actions CI, correlation IDs, structured error handling and production health monitoring.

## GitHub Repository Description

Short repository description:

Production-style railway journey search and booking API built with C#/.NET 8, PostgreSQL, EF Core and Docker, featuring concurrency-safe booking, automated testing, CI/CD and production observability.

Suggested repository topics:

csharp
dotnet
aspnet-core
postgresql
entity-framework-core
rest-api
docker
xunit
backend
clean-architecture
github-actions

## Interview Project Introduction

A concise way to introduce the project in an interview:

I built a railway journey search and booking backend in C# and .NET 8. I wanted it to go beyond a normal CRUD portfolio project, so I focused on problems that occur in real backend systems: transactional consistency, concurrent booking, external-service boundaries, integration testing, containerisation, CI and production observability.

The part I'd highlight most is the booking concurrency. Journeys use PostgreSQL optimistic concurrency, and I have an HTTP-level integration test where two customers simultaneously attempt to book the final available seat. Exactly one receives 201 Created, the other receives 409 Conflict, only one booking is persisted, and availability finishes at zero.

## Interview Talking Points

### Why did you use PostgreSQL in the integration tests?

The important persistence behaviours in the project include transactions, database constraints and optimistic concurrency.

Using an in-memory replacement would make the tests easier to run, but it would not exercise the same database behaviour as production.

For the important Infrastructure and API integration tests, I therefore chose to test against PostgreSQL.

### How do you prevent two customers from booking the final seat?

The journey is protected by optimistic concurrency using PostgreSQL's xmin row version.

If two operations read the same version of a journey and both attempt to reserve its final seat, only the first update can successfully persist that version.

The competing update encounters a concurrency conflict. The booking operation does not leave a partial booking behind, and the API represents the conflict as 409 Conflict.

I test this both at the persistence level and through two simultaneous HTTP requests.

### Why optimistic rather than pessimistic concurrency?

For this project I treated conflicting updates as exceptional rather than the normal case.

Optimistic concurrency avoids holding a pessimistic lock throughout the booking workflow while still detecting when another transaction has changed the journey before the update commits.

At significantly larger scale or under high contention, I would reevaluate that decision based on measured traffic and booking patterns.

### Why use separate Domain, Application, Infrastructure and API projects?

The separation keeps business behaviour independent from HTTP, Entity Framework Core and external services.

The Domain contains the core rules, Application coordinates use cases, Infrastructure implements technical dependencies, and API owns the HTTP boundary.

It also makes individual layers easier to test and prevents persistence or transport concerns from spreading throughout the solution.

### Why are the carrier providers simulated?

The goal was to demonstrate the external-service boundary rather than make the portfolio project dependent on a commercial railway API.

ICarrierJourneyProvider allows multiple providers to sit behind one application-facing contract.

The simulated Avanti and LNER providers demonstrate asynchronous calls, cancellation and provider failure handling while keeping tests deterministic.

A real provider could later perform HTTP calls and translate the external response behind the same boundary.

### How did you approach error handling?

Expected application failures are translated into appropriate HTTP responses using ProblemDetails.

Unexpected exceptions return a generic 500 Internal Server Error response so implementation details and stack traces are not leaked to clients.

The actual exception is still logged server-side.

Correlation IDs allow a client-visible failure to be connected with the relevant server-side request and error logs.

### What does your CI pipeline verify?

GitHub Actions restores the solution, performs a Release build with warnings treated as errors, runs the Domain, Application, Infrastructure and API test projects, and builds the API Docker image.

The PostgreSQL-dependent tests execute against a PostgreSQL service container in CI.

That means the pipeline verifies not only compilation and unit tests but also the real database integration and deployable container image.

## Important Engineering Trade-offs

### Automatic Migrations on Startup

The application automatically applies EF Core migrations during startup outside the Testing environment.

This makes local Docker and portfolio deployment straightforward because a clean database can initialise itself automatically.

For a larger production system, I would generally separate schema deployment from application startup so that migrations could be controlled, reviewed and rolled out independently.

### Simulated Carrier Integrations

Simulated providers make the repository deterministic and avoid credentials, rate limits and third-party availability.

The trade-off is that the project does not demonstrate actual external HTTP integration.

The provider boundary was designed so that real HTTP-backed implementations could be added without changing the core architecture.

### Cancellation Does Not Restore Availability

In the current project, cancelling a confirmed booking changes the booking status but does not return a seat to journey availability.

This is a known scope decision rather than behaviour that should be assumed for a real ticketing platform.

A production system would require explicit inventory-release and refund rules, with tests around their transactional behaviour.

### Single Application Deployment

The project is intentionally a single deployable backend rather than a collection of microservices.

For the project's current scale this keeps deployment, transactions and development straightforward while still allowing internal responsibilities to be separated cleanly.

Splitting the application into distributed services without a demonstrated scaling or organisational requirement would add complexity without automatically improving the design.

## What I Would Add for a Real Production Platform

If evolving the project toward a real large-scale railway booking platform, areas I would investigate include:

authentication and authorisation;

customer account management;

real carrier API integrations;

payment processing;

temporary seat or fare holds;

cancellation inventory restoration and refunds;

idempotency for booking/payment requests;

distributed caching;

API rate limiting;

provider-specific retries and circuit breakers;

metrics, tracing and centralised logging;

secret-management infrastructure;

controlled database migration deployment;

load and performance testing;

reservation expiry;

background processing where appropriate.

These would be introduced based on concrete system requirements rather than adding distributed architecture solely for complexity.

## Key Interview Message

The project is not intended to reproduce the complete functionality or scale of a commercial railway platform.

Its purpose is to demonstrate the engineering decisions behind a reliable backend:

Business rules
      +
Layered architecture
      +
PostgreSQL persistence
      +
Transactional consistency
      +
Concurrency protection
      +
External-service boundaries
      +
Automated testing
      +
Docker / CI
      +
Production observability
      +
Real deployment

The strongest example is the last-seat booking scenario because it connects the Domain, Application, Infrastructure, PostgreSQL and HTTP layers in one measurable reliability requirement.