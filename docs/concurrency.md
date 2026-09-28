# Booking Concurrency

## Overview

The Railway API prevents multiple customers from successfully booking the same final available seat.

A journey stores its current number of available seats. When a booking is created, the application validates availability, reserves a seat on the journey, and persists the journey update and booking within the same database transaction.

## Optimistic Concurrency

Journey updates use PostgreSQL optimistic concurrency through the `xmin` system column.

The EF Core journey configuration maps `xmin` as a row-version concurrency token:

```csharp
builder.Property<uint>("xmin").IsRowVersion();