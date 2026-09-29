# Docker and Local Deployment

The Railway backend can be run locally as a containerised application using Docker Compose.

The local deployment consists of two containers:

- **Railway API** — ASP.NET Core .NET 8 REST API.
- **PostgreSQL** — PostgreSQL 16 database.

Docker Compose manages the containers, networking, startup ordering, environment configuration, and database persistence.

## Architecture

The Docker Compose environment contains the following services:

### API

The Railway API is built from `Railway.Api/Dockerfile`.

The Dockerfile uses a multi-stage build:

1. The .NET 8 SDK image restores dependencies and publishes the application.
2. The final image uses the smaller ASP.NET Core 8 runtime image.
3. Only the published application is copied into the runtime image.
4. The application runs as a non-root user.
5. The API listens on container port `8080`.

The API is available from the host at:

`http://localhost:8080`

### PostgreSQL

The database runs using the official PostgreSQL 16 Docker image.

PostgreSQL is only accessible through the internal Docker Compose network and is not exposed directly to the host.

A health check verifies that PostgreSQL is ready before the API is started.

### Container Networking

Docker Compose creates a private network for the application.

The API connects to PostgreSQL using the Compose service name:

`postgres`

This means the containerised connection string uses `Host=postgres` rather than `Host=localhost`.

### Database Initialisation

When the API starts outside the Testing environment, it:

1. Applies pending EF Core migrations.
2. Seeds the database when required.
3. Starts serving HTTP requests.

This allows a new database to be initialised automatically when the Docker Compose stack is started from a clean state.

### Database Persistence

PostgreSQL data is stored in the named Docker volume:

`railway-postgres-data`

The database therefore survives normal container removal and recreation.

Running `docker compose down` removes the containers and network but preserves the database volume.

Running `docker compose down -v` also removes the database volume and should only be used when a completely clean database is required.

## Environment Variables

Docker Compose reads local database configuration from a `.env` file in the solution root.

The required variables are:

| Variable | Description |
| --- | --- |
| `POSTGRES_DB` | Name of the PostgreSQL database. |
| `POSTGRES_USER` | PostgreSQL user used by the application. |
| `POSTGRES_PASSWORD` | Password for the PostgreSQL user. |

Example `.env` configuration:

```env
POSTGRES_DB=railway
POSTGRES_USER=railway
POSTGRES_PASSWORD=your_local_password

## Docker Compose Commands

Run these commands from the solution root.

### Build the containers

```powershell
docker compose build
```

To force a completely fresh build without using the Docker build cache:

```powershell
docker compose build --no-cache
```

### Start the application

Build and start the complete stack:

```powershell
docker compose up -d --build
```

The API will be available at:

`http://localhost:8080`

### Check container status

```powershell
docker compose ps
```

The API should be running and PostgreSQL should report a healthy status.

### View API logs

```powershell
docker compose logs api
```

To view the most recent API log entries:

```powershell
docker compose logs --tail=50 api
```

### Stop the application

Stop and remove the containers and Compose network while preserving the PostgreSQL data volume:

```powershell
docker compose down
```

### Reset the database

To remove the containers and PostgreSQL data volume:

```powershell
docker compose down -v
```

The next startup will create a clean database and the API will automatically apply EF Core migrations and seed data.

### Clean rebuild

To test the application from a completely clean Docker environment:

```powershell
docker compose down -v
docker compose build --no-cache
docker compose up -d
docker compose ps
```