# Railway Backend API

## Overview

Railway Backend exposes a REST API for searching railway journeys and managing bookings.

Production base URL:

```text
https://railwaybackend-qf51.onrender.com
```

When running locally with Docker Compose:

```text
http://localhost:8080
```

All application endpoints are located beneath `/api`.

## HTTP Conventions

The API uses standard HTTP status codes to communicate request outcomes.

Common responses include:

| Status | Meaning |
|---|---|
| `200 OK` | Request completed successfully |
| `201 Created` | A booking was successfully created |
| `204 No Content` | A booking was successfully cancelled |
| `400 Bad Request` | The request was invalid |
| `404 Not Found` | The requested resource could not be found |
| `409 Conflict` | The request conflicts with the current application state |
| `500 Internal Server Error` | An unexpected server error occurred |

JSON is used for request and response bodies where applicable.

## Correlation IDs

The API supports request correlation through:

```text
X-Correlation-ID
```

Clients may provide their own correlation ID:

```http
X-Correlation-ID: example-request-123
```

If no ID is supplied, the API generates one.

The correlation ID is returned in the response header and is included in application request/error logging.

Handled exception responses also include the correlation ID in their `ProblemDetails` body, allowing a client-visible error to be matched with the corresponding server-side logs.

Example:

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Customer ID is required.",
  "correlationId": "example-request-123"
}
```

---

# Journey Endpoints

## Search Journeys

Search for railway journeys between two stations.

```http
GET /api/journeys/search
```

### Query Parameters

The search request supplies:

- Origin station
- Destination station
- Departure date

Example:

```http
GET /api/journeys/search?originStationId={originStationId}&destinationStationId={destinationStationId}&departureDate={departureDate}
```

The station identifiers are GUIDs representing persisted railway stations.

The endpoint returns journeys matching the requested route/date criteria.

### Example

```http
GET /api/journeys/search?originStationId=36ad81b9-7593-4329-a4bd-85b754046249&destinationStationId=8154a784-1954-404b-89d6-30dfae59fb7f&departureDate={departureDate}
```

The example station IDs represent the seeded London Euston and Birmingham stations in the deployed environment.

### Successful Response

```text
200 OK
```

The response contains the journeys available for the requested search.

### Invalid Search

Invalid request parameters result in:

```text
400 Bad Request
```

---

## Get Journey

Retrieve a specific persisted journey.

```http
GET /api/journeys/{id}
```

### Path Parameter

| Parameter | Description |
|---|---|
| `id` | Journey GUID |

Example:

```http
GET /api/journeys/9f3392ce-b82c-490f-9dd4-65fbea4a19ff
```

### Successful Response

```text
200 OK
```

The response contains the requested journey, including its journey information and available fares.

### Journey Not Found

If the journey does not exist:

```text
404 Not Found
```

The endpoint uses the standard ASP.NET Core problem response for this controller-level not-found result.

---

## Journey Availability

Journey data includes the number of seats currently remaining.

Availability is persisted in PostgreSQL and is updated when a booking successfully reserves a seat.

Booking concurrency protection ensures that competing requests cannot both successfully consume the final available seat. If availability changes before a booking can complete, the booking endpoint returns `409 Conflict` rather than allowing the journey to be oversold.

# Booking Endpoints

## Create Booking

Create a booking for a customer, journey, and fare.

```http
POST /api/bookings
Content-Type: application/json
```

### Request Body

```json
{
  "customerId": "customer-guid",
  "journeyId": "journey-guid",
  "fareId": "fare-guid"
}
```

All three identifiers are required.

### Successful Response

A successfully created booking returns:

```text
201 Created
```

The booking is persisted in PostgreSQL and the journey's remaining seat count is reduced as part of the booking operation.

### Invalid Request

Invalid booking data returns:

```text
400 Bad Request
```

For example, an empty customer identifier produces a handled validation error.

Example `ProblemDetails` response:

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Customer ID is required.",
  "correlationId": "example-request-123"
}
```

### Booking Conflict

If the booking cannot be completed because the requested journey no longer has sufficient availability:

```text
409 Conflict
```

This is particularly important when multiple requests compete for the final available seat. The concurrency controls ensure that only one request can successfully reserve that seat.

---

## Get Booking

Retrieve an existing booking by its identifier.

```http
GET /api/bookings/{id}
```

### Path Parameter

| Parameter | Description |
|---|---|
| `id` | Booking GUID |

Example:

```http
GET /api/bookings/dd175b10-0165-4113-b543-e9352ff5a827
```

### Successful Response

```text
200 OK
```

The response contains the persisted booking information, including its current booking status.

### Booking Not Found

If no booking exists with the supplied identifier:

```text
404 Not Found
```

---

## Cancel Booking

Cancel an existing booking.

```http
DELETE /api/bookings/{id}
```

### Path Parameter

| Parameter | Description |
|---|---|
| `id` | Booking GUID |

Example:

```http
DELETE /api/bookings/dd175b10-0165-4113-b543-e9352ff5a827
```

### Successful Response

```text
204 No Content
```

The booking remains persisted but its status is changed from confirmed to cancelled.

A subsequent:

```http
GET /api/bookings/{id}
```

returns the booking with its cancelled status.

> Cancellation does not currently restore a seat to the journey. This is the intentional behaviour of the current implementation.

### Booking Not Found

Attempting to cancel a booking that cannot be found results in the application's corresponding not-found response.

---

# Error Responses

## ProblemDetails

Handled application exceptions are represented using ASP.NET Core `ProblemDetails`.

A handled error can contain:

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Description of the request failure.",
  "correlationId": "request-correlation-id"
}
```

Expected application failures are mapped to HTTP status codes including:

| Application Condition | HTTP Status |
|---|---|
| Invalid request or operation | `400 Bad Request` |
| Resource not found | `404 Not Found` |
| Booking conflict/unavailable | `409 Conflict` |
| Unexpected exception | `500 Internal Server Error` |

## Unexpected Errors

Unexpected exceptions return:

```text
500 Internal Server Error
```

The client receives a deliberately generic error:

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred.",
  "correlationId": "request-correlation-id"
}
```

Internal exception messages and stack traces are not exposed to the API consumer.

The actual exception is logged server-side as an error together with the request correlation ID, allowing the failure to be investigated without leaking implementation details to clients.

## Error Logging

Expected handled application errors such as `400`, `404`, and `409` are logged as warnings when they pass through the global exception handler.

Unexpected `500` failures are logged as errors with their underlying exception information.

Request logging independently records the final HTTP response status, execution time, request path, and correlation ID.