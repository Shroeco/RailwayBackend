# Railway Backend Architecture



## Overview



Railway Backend uses a layered architecture that separates domain rules, application use cases, infrastructure concerns, and HTTP delivery.



The architecture is designed to keep the core railway and booking behaviour independent from PostgreSQL, Entity Framework Core, ASP.NET Core, and external carrier implementations.



```text

┌──────────────────────────────┐

│         Railway.Api          │

│ Controllers / Middleware /   │

│ Health / Exception Handling  │

└──────────────┬───────────────┘

               │

               ▼

┌──────────────────────────────┐

│      Railway.Application     │

│ Services / DTOs / Use Cases  │

│ Repository Abstractions      │

└──────────────┬───────────────┘

               │

               ▼

┌──────────────────────────────┐

│        Railway.Domain        │

│ Entities / Business Rules    │

└──────────────────────────────┘

               ▲

               │

┌──────────────┴───────────────┐

│    Railway.Infrastructure    │

│ EF Core / PostgreSQL /       │

│ Repositories / Carriers      │

└──────────────────────────────┘

```



## Layer Responsibilities



### Railway.Domain



`Railway.Domain` contains the core business model.



Its entities represent concepts including:



- Stations

- Carriers

- Journeys

- Journey legs

- Fares

- Customers

- Bookings



Business behaviour that belongs to these concepts is kept within the domain rather than being implemented in controllers or database-specific code.



The Domain project does not need to understand HTTP, ASP.NET Core, Entity Framework Core, PostgreSQL, or deployment infrastructure.



### Railway.Application



`Railway.Application` coordinates the application's use cases.



Its responsibilities include:



- Journey search and retrieval

- Booking creation

- Booking retrieval

- Booking cancellation

- Application DTOs

- Service orchestration

- Persistence abstractions



The Application layer coordinates domain behaviour without needing to know how persistence is technically implemented.



This means application services can depend on abstractions while `Railway.Infrastructure` provides their concrete implementations.



### Railway.Infrastructure



`Railway.Infrastructure` contains implementations for concerns outside the application's core business rules.



Its responsibilities include:



- Entity Framework Core

- PostgreSQL persistence

- `RailwayDbContext`

- Repository implementations

- Transactional booking persistence

- Database migrations

- Database seed data

- Simulated external carrier providers



Infrastructure therefore handles the technical details required to persist and retrieve the application's domain state.



### Railway.Api



`Railway.Api` is the HTTP entry point and composition root.



Its responsibilities include:



- ASP.NET Core controllers

- Dependency injection configuration

- HTTP request/response handling

- Global exception handling

- Request logging

- Correlation IDs

- Health checks

- Application startup

- Database migration during startup



Controllers remain focused on HTTP concerns and delegate application behaviour to services rather than directly implementing persistence or domain workflows.



## Dependency Direction



The architecture deliberately keeps dependencies pointing toward the application's core.



```text

Railway.Api

    │

    ├──────────────► Railway.Application

    │

    └──────────────► Railway.Infrastructure

                         │

                         ▼

Railway.Application ──► Railway.Domain

Railway.Infrastructure ► Railway.Domain

```



The important distinction is between ****compile-time dependency direction**** and ****runtime execution****.



For example, the Application layer can define an abstraction required by a use case while Infrastructure supplies its implementation. ASP.NET Core dependency injection connects the abstraction and implementation when the application starts.



This prevents application services from being tightly coupled to Entity Framework Core or PostgreSQL.



## Request Flow



A typical HTTP operation follows this path:



```text

HTTP Request

     │

     ▼

ASP.NET Core Middleware

     │

     ├── Correlation ID

     ├── Request Logging

     └── Exception Handling

     │

     ▼

Controller

     │

     ▼

Application Service

     │

     ├── Domain behaviour

     │

     └── Repository abstraction

              │

              ▼

     Infrastructure Repository

              │

              ▼

         PostgreSQL

     │

     ▼

HTTP Response

```



This separation allows each part of the request lifecycle to have a focused responsibility.



## Journey Search Flow



Journey search is coordinated through the Application layer while carrier-specific behaviour remains behind provider abstractions.



```text

GET /api/journeys/search

          │

          ▼

JourneysController

          │

          ▼

JourneyService

          │

          ▼

ICarrierJourneyProvider

          │

     ┌────┴────┐

     ▼         ▼

  Avanti      LNER

 Provider    Provider

     │         │

     └────┬────┘

          ▼

 Journey Results

          │

          ▼

    HTTP Response

```



`ICarrierJourneyProvider` provides an abstraction over carrier journey sources. The Infrastructure layer supplies simulated Avanti and LNER implementations.



This keeps carrier-specific logic outside controllers and application services and provides an extension point for replacing simulated providers with real external railway integrations.



Provider operations are asynchronous and support cancellation, allowing the application to handle external-style operations without blocking request processing.



## Booking Flow



Booking creation crosses several architectural boundaries and is one of the most important workflows in the application.



```text

POST /api/bookings

        │

        ▼

BookingsController

        │

        ▼

BookingService

        │

        ├── Validate request/use case

        │

        ▼

Booking Repository

        │

        ▼

PostgreSQL Transaction

        │

        ├── Check/reserve availability

        ├── Update remaining seats

        └── Persist booking

        │

        ▼

Transaction Commit

        │

        ▼

201 Created

```



The controller handles the HTTP request while the Application service coordinates the booking use case. Persistence and transaction management are delegated to the Infrastructure layer.



A successful operation persists the booking and updates journey availability as one coordinated database operation.



Failures are translated at the API boundary into appropriate HTTP responses, including `409 Conflict` when a booking cannot be completed because availability has changed.



## Persistence Architecture



PostgreSQL is the persistent data store for the application, with Entity Framework Core providing object-relational mapping and database access.



```text

Application

     │

     ▼

Repository Abstraction

     │

     ▼

Infrastructure Repository

     │

     ▼

RailwayDbContext

     │

     ▼

Entity Framework Core

     │

     ▼

PostgreSQL

```



This design prevents application services from depending directly on `RailwayDbContext`.



Database schema evolution is managed through Entity Framework Core migrations. Seed data provides the railway data required for development and deployed environments.



Integration tests also use PostgreSQL rather than substituting an in-memory database. This is particularly important for testing transaction and concurrency behaviour that depends on the real database.



## Concurrency and Transaction Design



The booking workflow is designed to prevent two concurrent requests from both successfully purchasing the final available seat.



A simple application-level sequence such as:



```text

Read seats remaining

        │

        ▼

Check seats > 0

        │

        ▼

Create booking

        │

        ▼

Decrease seats

```



would be vulnerable to a race condition if two requests observed the same availability before either update was persisted.



The Railway Backend instead coordinates booking persistence and the seat update transactionally through PostgreSQL.



Conceptually, when two requests compete for one seat:



```text

                    Journey

                 Seats Remaining: 1

                        │

              ┌─────────┴─────────┐

              ▼                   ▼

         Request A            Request B

              │                   │

              └──────► PostgreSQL ◄──────┘

                        │

                 concurrency control

                        │

              ┌─────────┴─────────┐

              ▼                   ▼

        Booking created      Booking rejected

        Seats: 1 → 0          409 Conflict

```



The final state must therefore satisfy the following invariants:



- Exactly one booking is created.

- Remaining seats become zero.

- Remaining seats never become negative.

- The competing booking request fails rather than overselling the journey.



This behaviour is verified at both persistence and HTTP levels, including an API test that sends two booking requests concurrently against a journey containing exactly one remaining seat.



The test then reloads the database state through a fresh context and confirms that only one booking exists and that no seats remain.



## External Carrier Design



Carrier integrations follow an abstraction-based design:



```text

              Application

                   │

                   ▼

       ICarrierJourneyProvider

                   │

          ┌────────┴────────┐

          ▼                 ▼

   Avanti Provider      LNER Provider

```



The current providers simulate external carrier systems, allowing provider selection, asynchronous behaviour, cancellation, and failure scenarios to be exercised without depending on live third-party APIs.



The important architectural boundary is the provider interface rather than the simulated implementation itself. A future implementation could call a real carrier API while preserving the Application layer's use of the same abstraction.



## Production Architecture



The deployed application remains deliberately simple:



```text

            Client

              │

           HTTPS

              │

              ▼

          Render

     ┌─────────────────┐

     │ Railway.Api     │

     │ Docker Container│

     └────────┬────────┘

              │

              ▼

     Managed PostgreSQL

```



Render terminates HTTPS before forwarding traffic to the HTTP endpoint exposed by the application container.



Production database configuration is supplied through environment variables rather than committed configuration files.



At application startup, database migrations are applied before the API begins normal operation.



Operational endpoints provide separate health concepts:



```text

/health

   └── API + PostgreSQL



/health/live

   └── API process



/health/ready

   └── PostgreSQL-backed readiness

```



Request logging, correlation IDs, global exception handling, and production-specific logging configuration provide the operational visibility required to diagnose requests without exposing internal exception information to API consumers.



## Architectural Trade-offs



The project intentionally uses a modular monolithic architecture rather than splitting functionality into microservices.



For the current scope, this provides:



- Clear separation of concerns without distributed-system complexity.

- Straightforward transactional consistency for booking operations.

- Simple local development and deployment.

- A single CI/CD pipeline.

- Easier end-to-end and integration testing.



Several production-scale concerns are deliberately outside the scope of the project, including real payment processing, authentication and customer accounts, real railway APIs, distributed messaging, and independently deployed microservices.



The architecture nevertheless establishes boundaries around persistence and external carrier integrations so these areas can evolve without moving their implementation details into the core business logic.
