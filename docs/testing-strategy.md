# Testing Strategy

## Overview

The Railway backend uses automated tests across the domain, application, infrastructure, and API layers.

The goal is to verify business rules in isolation while also testing the behaviour of the complete application against a real PostgreSQL database.

The test suite currently contains 85 tests across four test projects:

- `Railway.Domain.Tests`
- `Railway.Application.Tests`
- `Railway.Infrastructure.Tests`
- `Railway.Api.Tests`

## Domain Tests

Domain tests verify the core business rules implemented by the domain entities.

These tests cover behaviour such as:

- entity validation;
- journey seat reservation;
- prevention of invalid state;
- booking state transitions;
- cancellation rules.

Domain tests do not require infrastructure or a database and therefore provide fast feedback for core business behaviour.

## Application Tests

Application tests verify the behaviour of application services and use-case coordination.

Dependencies such as repositories are replaced with test implementations so that application behaviour can be tested independently from PostgreSQL.

Important scenarios include:

- valid journey searches;
- invalid journey search parameters;
- successful booking creation;
- missing customers;
- missing journeys;
- invalid fare and journey combinations;
- journeys with no available seats;
- successful booking cancellation;
- missing bookings;
- duplicate cancellation;
- empty identifiers.

These tests verify that the application layer applies the expected validation and business workflow before persistence.

## Infrastructure Tests

Infrastructure tests run against a real PostgreSQL database.

Using PostgreSQL rather than an EF Core in-memory replacement ensures that tests exercise database behaviour used by the actual application.

Infrastructure tests cover:

- repository reads and writes;
- journey persistence;
- booking persistence;
- booking retrieval;
- cancellation persistence;
- database constraints;
- transactional rollback;
- optimistic concurrency.

Test data uses unique identifiers and values so that individual tests remain isolated.

Cleanup operations remove only data owned by the individual test.

## API Integration Tests

API integration tests use ASP.NET Core `WebApplicationFactory` to run the application through its real HTTP pipeline.

The application runs using the `Testing` environment and connects to the PostgreSQL integration-test database.

Automatic development seed data is disabled in this environment so that tests control their own database state.

The API tests cover:

- journey search;
- journey details;
- booking creation;
- booking retrieval;
- booking cancellation;
- invalid requests;
- `404 Not Found`;
- `409 Conflict`;
- `ProblemDetails` error responses;
- correlation ID propagation;
- expected error observability;
- safe handling of unexpected `500 Internal Server Error` responses.

Where appropriate, tests verify both the HTTP response and the resulting state in PostgreSQL using a fresh database context.

## Error Observability Testing

The API test suite verifies the production error-handling behaviour introduced as part of the application's observability work.

A handled invalid booking request is tested with a caller-supplied `X-Correlation-ID`.

The test verifies that:

- the request returns `400 Bad Request`;
- the supplied correlation ID is returned in the response header;
- the response uses `ProblemDetails`;
- the response contains the expected validation message;
- the same correlation ID is included in the `ProblemDetails` extensions.

Unexpected exceptions are tested separately by replacing the booking service with a controlled test implementation that throws an exception.

The test verifies that:

- the API returns `500 Internal Server Error`;
- the correlation ID is preserved;
- the client receives the generic message `An unexpected error occurred.`;
- the original internal exception message is not exposed in the response.

These tests ensure that production failures remain diagnosable through correlation IDs while sensitive implementation details are not leaked to API consumers.

## Concurrency Testing

Booking concurrency is an important reliability requirement because multiple customers may attempt to reserve the final available seat simultaneously.

Concurrency is tested at multiple levels.

Infrastructure tests verify PostgreSQL/EF Core optimistic concurrency and transactional behaviour.

An API integration test sends two concurrent booking requests for a journey with exactly one available seat.

The test verifies that:

- exactly one request succeeds with `201 Created`;
- exactly one request receives `409 Conflict`;
- exactly one booking is persisted;
- the journey finishes with zero available seats;
- available seats cannot become negative;
- the failed request leaves no partial booking.

The implementation and concurrency behaviour are described in more detail in `docs/concurrency.md`.

## Test Isolation

Tests must not depend on execution order or shared seed data.

Database tests therefore:

- create their own test data;
- use unique GUIDs and other unique values;
- clean up only their own records;
- use fresh database contexts when verifying persisted state;
- use `try/finally` where cleanup must occur even after a failed assertion.

This allows tests to remain deterministic while sharing the PostgreSQL integration-test database.

## Test Quality

Tests use descriptive names that communicate the operation being tested and the expected behaviour.

A typical naming pattern is:

`Method_should_expected_behaviour_when_condition`

Tests are structured around Arrange, Act, and Assert even where explicit comments are unnecessary.

The suite prioritises meaningful behaviour and important failure scenarios rather than increasing the test count for its own sake.

## Running the Tests

From the solution root:

```powershell
dotnet test