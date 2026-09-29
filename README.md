# Railway Backend

A production-style railway journey search and booking REST API built with **C#**, **.NET 8**, **ASP.NET Core**, **Entity Framework Core**, and **PostgreSQL**.

The project models the backend of a railway booking platform, with a focus on backend engineering practices rather than frontend functionality. It supports journey search, fare selection, booking creation and cancellation, external carrier abstraction, and concurrency-safe seat reservation.

The API is containerised with Docker, tested across domain, application, infrastructure, and HTTP layers, continuously validated through GitHub Actions, and deployed to Render with a managed PostgreSQL database.

## Live API

The production API is deployed on Render:

**https://railwaybackend-qf51.onrender.com**

Health endpoints:

- `/health` — application and database health
- `/health/live` — application liveness
- `/health/ready` — database-backed readiness

> The Render free tier may spin the service down after inactivity, so the first request can take longer while the service starts.

## Key Features

- Journey search between railway stations
- Journey and fare retrieval
- Booking creation and persistence
- Booking cancellation
- Concurrency-safe seat reservation
- Transactional booking persistence
- External carrier provider abstraction
- PostgreSQL persistence with Entity Framework Core
- Database migrations and seed data
- Structured request and error logging
- Request correlation using `X-Correlation-ID`
- Liveness and readiness health checks
- Consistent `ProblemDetails` error responses
- Docker-based local deployment
- GitHub Actions CI pipeline
- Production deployment on Render
- 85 automated tests across four test projects

## Technology Stack

| Area | Technology |
|---|---|
| Language | C# |
| Framework | .NET 8 / ASP.NET Core |
| Persistence | Entity Framework Core |
| Database | PostgreSQL |
| API | REST / ASP.NET Core Controllers |
| Testing | xUnit |
| Containerisation | Docker / Docker Compose |
| CI | GitHub Actions |
| Production Hosting | Render |
| API Documentation | Swagger / OpenAPI (Development) |

## Architecture

The solution follows a layered architecture designed to separate business rules, application use cases, persistence concerns, and HTTP delivery.

```text
Railway.Api
    │
    ▼
Railway.Application
    │
    ▼
Railway.Domain

Railway.Infrastructure
    │
    ├── implements Application repository abstractions
    ├── PostgreSQL / Entity Framework Core
    └── external carrier providers

### `Railway.Domain`

Contains the core railway and booking model, including entities such as journeys, journey legs, fares, customers, bookings, stations, and carriers. Domain behaviour such as reserving seats and booking state is kept independent from HTTP and database concerns.

### `Railway.Application`

Coordinates application use cases such as journey retrieval, booking creation, and booking cancellation. It defines service and repository abstractions while depending on the Domain layer rather than infrastructure implementations.

### `Railway.Infrastructure`

Provides the technical implementations required by the application, including Entity Framework Core repositories, PostgreSQL persistence, transactional booking operations, and simulated external railway carrier providers.

### `Railway.Api`

Exposes the application through REST endpoints using ASP.NET Core controllers. It also contains API-specific concerns such as exception handling, request logging, correlation IDs, health checks, dependency injection, and application startup.

This separation keeps the core business logic independent from persistence and delivery mechanisms while allowing infrastructure components to be replaced behind abstractions.

## Carrier Provider Abstraction

Journey data is accessed through the `ICarrierJourneyProvider` abstraction, allowing the application to work with multiple railway carriers without coupling application logic to a specific provider.

The Infrastructure layer currently contains simulated providers for:

- Avanti
- LNER

Providers are registered through dependency injection and use asynchronous operations. This design provides a clear extension point where simulated providers could later be replaced by integrations with real external railway APIs without restructuring the application layer.

## Testing

The solution contains four dedicated test projects:

| Test Project | Purpose |
|---|---|
| `Railway.Domain.Tests` | Tests domain entities, validation, and business rules |
| `Railway.Application.Tests` | Tests application services and use-case behaviour |
| `Railway.Infrastructure.Tests` | Tests PostgreSQL persistence, transactions, repositories, and concurrency |
| `Railway.Api.Tests` | Tests the application through HTTP endpoints |

The current test suite contains **85 automated tests**.

Testing covers areas including:

- Domain validation and behaviour
- Journey and booking services
- Repository persistence
- Transaction rollback
- PostgreSQL integration
- Booking cancellation
- HTTP status codes and responses
- Last-seat concurrency
- Error handling
- Correlation ID propagation
- Safe `ProblemDetails` responses
- Unexpected server-error handling

Integration and API tests use PostgreSQL rather than replacing the persistence layer with an in-memory database, allowing database-specific transactional and concurrency behaviour to be tested.

Run the complete test suite with:

```bash
dotnet test --configuration Release
```

A warning-free Release build can be verified with:

```bash
dotnet build --configuration Release --warnaserror
```

## Observability and Error Handling

The API includes several features intended to make production behaviour easier to diagnose.

Every request is logged with its HTTP method, path, response status, execution time, and correlation ID.

Requests receive an `X-Correlation-ID`. A caller may provide an ID or allow the API to generate one automatically. The same identifier is propagated through request logs and handled error responses, allowing a request to be traced through the application.

Expected application failures are converted into appropriate HTTP responses such as:

- `400 Bad Request`
- `404 Not Found`
- `409 Conflict`

Unexpected exceptions return a generic `500 Internal Server Error` response without exposing internal exception details to the client. The underlying exception is retained in server-side logging for diagnosis.

The API also exposes separate health concepts:

- `/health` — application and PostgreSQL health
- `/health/live` — process liveness without database dependency
- `/health/ready` — readiness including PostgreSQL connectivity

This allows infrastructure to distinguish between an API process that is running and an application that is actually ready to serve database-backed requests.

## Docker

The API and PostgreSQL database can be run locally using Docker Compose.

The API uses a multi-stage Docker build:

1. The .NET 8 SDK image restores and publishes the application.
2. The published application is copied into the smaller ASP.NET Core runtime image.
3. The container exposes the API over port `8080`.

PostgreSQL runs in a separate container with persistent storage and a database health check.

## Continuous Integration

GitHub Actions automatically validates the project on pushes and pull requests to `master`.

The CI pipeline:

1. Checks out the repository.
2. Configures .NET 8.
3. Starts a PostgreSQL service container.
4. Restores dependencies.
5. Builds the solution in Release mode with warnings treated as errors.
6. Runs Domain tests.
7. Runs Application tests.
8. Runs Infrastructure tests.
9. Runs API tests.
10. Builds the Railway API Docker image.

This ensures that compilation, automated tests, PostgreSQL integration, and container creation are continuously verified.

## Production Deployment

The backend is deployed to **Render** using the repository's Docker configuration.

The production environment consists of:

- Docker-hosted ASP.NET Core API
- Managed PostgreSQL database
- Environment-based database configuration
- Automatic EF Core migrations and seed data
- Health monitoring
- Automatic deployment from the GitHub `master` branch

Production-specific logging suppresses routine Entity Framework SQL command output while retaining application request, warning, and error logs.

## Running Locally

### Prerequisites

To run the project locally, install:

- .NET 8 SDK
- Docker Desktop
- Git

### Clone the Repository

```bash
git clone https://github.com/Shroeco/RailwayBackend.git
cd RailwayBackend
```

### Configure the Database

The Docker Compose configuration reads PostgreSQL configuration from a root `.env` file.

Create:

```text
.env
```

with:

```env
POSTGRES_DB=railway
POSTGRES_USER=railway
POSTGRES_PASSWORD=your_local_password
```

The `.env` file is excluded from Git and should not be committed.

### Run with Docker Compose

Build and start PostgreSQL and the API:

```bash
docker compose up --build
```

The API will be available at:

```text
http://localhost:8080
```

Verify the deployment with:

```bash
curl http://localhost:8080/health
```

To stop the containers:

```bash
docker compose down
```

To also remove the local PostgreSQL volume:

```bash
docker compose down -v
```

> Removing the volume deletes the local containerised database and its persisted data.

## Example API Usage

### Search for Journeys

```http
GET /api/journeys/search?originStationId={stationId}&destinationStationId={stationId}&departureDate={date}
```

### Get a Journey

```http
GET /api/journeys/{journeyId}
```

### Create a Booking

```http
POST /api/bookings
Content-Type: application/json

{
  "customerId": "customer-guid",
  "journeyId": "journey-guid",
  "fareId": "fare-guid"
}
```

A successful booking returns:

```text
201 Created
```

If the requested seat is no longer available, the API returns:

```text
409 Conflict
```

### Get a Booking

```http
GET /api/bookings/{bookingId}
```

### Cancel a Booking

```http
DELETE /api/bookings/{bookingId}
```

A successful cancellation returns:

```text
204 No Content
```

## Documentation

More detailed technical documentation is available in the [`docs`](docs/) directory:

- [Architecture](docs/architecture.md)
- [API Reference](docs/api.md)
- [Testing Strategy](docs/testing-strategy.md)
- [Concurrency](docs/concurrency.md)
- [Carrier Integration](docs/carrier-integration.md)
- [Docker Setup](docs/docker-setup.md)
- [Portfolio Guide](docs/portfolio.md)

---

## Solution Structure

```text
RailwayBackend/
├── Railway.Api/
├── Railway.Application/
├── Railway.Domain/
├── Railway.Infrastructure/
├── Railway.Api.Tests/
├── Railway.Application.Tests/
├── Railway.Domain.Tests/
├── Railway.Infrastructure.Tests/
├── .github/
│   └── workflows/
├── docker-compose.yml
└── Railway.sln
```

## Engineering Highlights

This project was built to demonstrate more than basic CRUD API development. Particular emphasis was placed on:

- Layered architecture and separation of concerns
- Transactional PostgreSQL persistence
- Concurrency-safe booking behaviour
- External service abstraction
- Integration testing against a real database
- HTTP-level concurrency testing
- Automated CI validation
- Containerised deployment
- Production configuration and secret separation
- Structured logging and request correlation
- Liveness and readiness monitoring
- Safe production error handling

The result is a backend designed around the kinds of reliability, testing, deployment, and operational concerns that become important beyond a purely local application.