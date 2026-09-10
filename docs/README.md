# Implementation notes

## Dependency flow

The application keeps the dependency chain simple and explicit:

Web -> Services -> Data service -> EF Core DbContext

- The MVC layer only talks to the application service abstraction.
- The application service depends on the data service contract, not on EF entities.
- The data service is responsible for querying and projecting EF entities into Data transfer models.
- EF Core entities remain private to the Data project, and the public data boundary returns materialised models instead of `IQueryable`.

This keeps the web layer decoupled from persistence details while preserving the existing layered architecture.

## Validation and normalisation

The current model still follows the existing validation approach already present in the project:

- `Forename` and `Surname` are required and trimmed.
- `Email` is required, trimmed and validated using the existing email attribute.
- The app uses a normalised email string for uniqueness checks and indexing.
- The Data layer is the persistence boundary; it normalises values before saving wherever the model is directly mutated.

## Async and data-access behaviour

This branch specifically addresses the asynchronous-query path:

- `DataContext` is configured through `DbContextOptions<DataContext>`.
- The application registers the concrete `DataContext` with the DI container.
- `UserManagementDataService.GetUsersAsync` uses EF Core async query operators and `AsNoTracking()` for read-only access.
- Filtering is evaluated in the Data project before materialising the final DTO list.

This keeps the read path asynchronous end-to-end and avoids wrapping synchronous query execution in `Task.FromResult`.

## Temporary in-memory configuration

The app still uses the EF Core InMemory provider as a temporary development stand-in while the PostgreSQL branch is not yet in place. This is intentional and is isolated in the Data layer registration so the rest of the application remains provider-agnostic.

## Local verification

Use the solution commands from the root:

```bash
dotnet restore UserManagement.slnx
dotnet build UserManagement.slnx --configuration Release
dotnet test UserManagement.slnx --configuration Release --no-build
```

> Note: in this environment, `dotnet test` is blocked by the .NET 10 / Microsoft.Testing.Platform VSTest compatibility issue, even though the project build succeeds. The underlying code path still builds cleanly, and the suite is ready for the intended .NET 10 test runner configuration.
