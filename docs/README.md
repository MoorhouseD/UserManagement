# Implementation notes

## Dependency flow

The application keeps the dependency chain simple and explicit:

Web -> Services -> Data service -> EF Core DbContext

- The MVC layer only talks to the application service abstraction.
- The application service depends on the data service contract, not on EF entities.
- The data service is responsible for querying and projecting EF entities into Data transfer models.
- EF Core entities remain private to the Data project, and the public data boundary returns materialised models instead of `IQueryable`.

This keeps the web layer decoupled from persistence details while preserving the existing layered architecture.

## Requirements

Install or have available:

- .NET 10 SDK `10.0.400` or a later feature-compatible SDK. The required SDK version is specified in `global.json`.
- Docker Desktop, Docker Engine, or another Docker-compatible runtime with Docker Compose support. This provides the PostgreSQL container used by the application.
- Git, if cloning or updating the repository from source control.

PostgreSQL does not need to be installed separately when using the included `docker-compose.yml`. The compose file starts PostgreSQL 16 on port `5432` with the development database and credentials expected by `appsettings.json`.

The .NET and EF Core package dependencies are restored by the project. The EF Core command-line tool is managed by the repository's local tool manifest; restore it with:

```bash
dotnet tool restore
```

On a new machine, trust the local ASP.NET Core HTTPS certificate if you want to use the HTTPS launch URL:

```bash
dotnet dev-certs https --trust
```

## Validation and normalisation

The current model still follows the existing validation approach already present in the project:

- `Forename` and `Surname` are required and trimmed.
- `Email` is required, trimmed and validated using the existing email attribute.
- The app uses a normalised email string for uniqueness checks and indexing.
- The Data layer is the persistence boundary; it normalises values before saving wherever the model is directly mutated.

## Edit and delete behaviour

- Users can be edited from the list through a validated GET/POST MVC flow.
- Updates use the same server-side validation as creation, including past-date-of-birth and unique-email checks. A user may keep their existing email address.
- Successful edits and deletes use redirect-after-POST and expose a success message through `TempData`.
- Deletion is POST-only and protected by antiforgery validation. The temporary InMemory database means deletions and edits reset when the application stops.

## User action logging

- Create, view, edit and delete actions are recorded with the user snapshot, action, details and UTC timestamp.
- A user's Details page includes their action history.
- The Logs page is available at `/logs`, supports page navigation, and links to individual log details at `/logs/{id}`.
- Logs use the same temporary InMemory database and reset when the application stops.

## Async and data-access behaviour

This branch specifically addresses the asynchronous-query path:

- `DataContext` is configured through `DbContextOptions<DataContext>`.
- The application registers the concrete `DataContext` with the DI container.
- `UserManagementDataService.GetUsersAsync` uses EF Core async query operators and `AsNoTracking()` for read-only access.
- Filtering is evaluated in the Data project before materialising the final DTO list.

This keeps the read path asynchronous end-to-end and avoids wrapping synchronous query execution in `Task.FromResult`.

## PostgreSQL configuration and migrations

The web application uses PostgreSQL through the `ConnectionStrings:UserManagement` setting. The repository includes a local Docker Compose database with matching development credentials:

```bash
docker compose up -d postgres
```

The application applies committed EF Core migrations at startup. Stop the local database with:

```bash
docker compose down
```

The unit test projects continue to create isolated EF Core InMemory contexts directly. The `UserManagement.Integration.Tests` project uses Testcontainers to start a disposable PostgreSQL 16 container, applies the committed migrations through the real web startup path and exercises selected HTTP journeys. Docker must be running for those tests.

### What migrations do

EF Core migrations are versioned descriptions of database schema changes. They keep the PostgreSQL schema aligned with the entity model as tables, columns, indexes and seed data evolve. The committed initial migration creates the `Users` and `UserActionLogs` tables and the unique normalised-email index.

The application calls `Database.Migrate()` during startup, so any committed migrations that have not yet been applied are run automatically after PostgreSQL is available. Migrations should be reviewed and committed with the model changes that require them.

### Create and inspect migrations

After changing the Data model, create a migration with a descriptive name:

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations add AddUserPhoneNumber \
	--project UserManagement.Data/UserManagement.Data.csproj \
	--startup-project UserManagement.Web/UserManagement.Web.csproj \
	--output-dir Migrations
```

List migrations and see which have been applied to the configured database:

```bash
dotnet tool run dotnet-ef migrations list \
	--project UserManagement.Data/UserManagement.Data.csproj \
	--startup-project UserManagement.Web/UserManagement.Web.csproj
```

Review the SQL that would be executed without changing the database:

```bash
dotnet tool run dotnet-ef migrations script \
	--project UserManagement.Data/UserManagement.Data.csproj \
	--startup-project UserManagement.Web/UserManagement.Web.csproj
```

### Apply and roll back migrations

Apply all pending migrations explicitly, for example in a deployment or local setup:

```bash
dotnet tool run dotnet-ef database update \
	--project UserManagement.Data/UserManagement.Data.csproj \
	--startup-project UserManagement.Web/UserManagement.Web.csproj
```

To roll back a database that has already applied the latest migration, update it to the previous migration by name:

```bash
dotnet tool run dotnet-ef database update PreviousMigrationName \
	--project UserManagement.Data/UserManagement.Data.csproj \
	--startup-project UserManagement.Web/UserManagement.Web.csproj
```

If the latest migration has not been applied to any shared database, remove its files instead:

```bash
dotnet tool run dotnet-ef migrations remove \
	--project UserManagement.Data/UserManagement.Data.csproj \
	--startup-project UserManagement.Web/UserManagement.Web.csproj
```

Do not remove a migration that has already been applied to a shared database. Roll the database back first, coordinate the change with other environments, and review the generated SQL because rollback can be destructive or require data restoration.

## Local verification

The application requires the .NET 10 SDK. Run these commands from the repository root.

### Run the web application

Start the web project with its development launch profile:

```bash
dotnet run --project UserManagement.Web/UserManagement.Web.csproj --launch-profile UserManagement.Web
```

Open <https://localhost:7084>, or go directly to the users page at <https://localhost:7084/users/List>. If the local HTTPS certificate is not trusted, use <http://localhost:5084/users/List> instead.

For automatic rebuilds while editing:

```bash
dotnet watch --project UserManagement.Web/UserManagement.Web.csproj run
```

The Data and Services projects are class libraries and run through the web project; they are not started independently. The PostgreSQL database persists data in the Docker volume until that volume is explicitly removed.

### Run the tests

Restore the solution and run all Data, Services and Web tests:

```bash
dotnet restore UserManagement.slnx
dotnet test UserManagement.slnx --configuration Release
```

Run only the PostgreSQL integration tests when iterating on relational or HTTP behaviour:

```bash
dotnet test UserManagement.Integration.Tests/UserManagement.Integration.Tests.csproj --configuration Release
```

The integration fixture removes its PostgreSQL container after the test run. Fast unit and MVC-controller tests use InMemory and cover service decisions without requiring Docker; integration tests cover migration application, relational persistence and the key HTTP boundary.

The repository includes `global.json` to opt `dotnet test` into the Microsoft.Testing.Platform runner required by the .NET 10 SDK.

To build without running tests:

```bash
dotnet build UserManagement.slnx --configuration Release --no-restore
```

## Container and health checks

Build the application image from the repository root:

```bash
docker build --tag usermanagement:local .
```

The image uses separate .NET SDK and ASP.NET runtime stages, publishes only the Web project output and runs as the non-root runtime user. PostgreSQL configuration is supplied at runtime; no credentials are copied into the image.

The application exposes two probe endpoints:

- `/health/live` reports process liveness and does not depend on PostgreSQL.
- `/health/ready` checks PostgreSQL connectivity and returns failure when the database is unavailable.

For a local container smoke test, start PostgreSQL and run the image on the Compose network:

```bash
docker compose up -d postgres
docker build --tag usermanagement:local .
docker run --rm --name usermanagement-app \
	--network techtest_default \
	--publish 8080:8080 \
	--env ConnectionStrings__UserManagement='Host=postgres;Port=5432;Database=usermanagement;Username=postgres;Password=postgres' \
	usermanagement:local
```

The CI workflow at `.github/workflows/ci.yml` restores, builds and tests the solution, uploads TRX test results, builds the image and smoke-tests `/health/live`. PostgreSQL integration tests use disposable Testcontainers instances on the hosted runner. The workflow does not publish an image or deploy infrastructure.
