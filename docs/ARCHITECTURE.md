# Maybeworks BIM AI Assistant Architecture

## Clean Architecture

The solution is structured around Clean Architecture boundaries:

- `Models`: domain/data transfer models used by the Revit plugin.
- `Application`: CQRS, repository, unit-of-work, validation, mapping and domain event abstractions.
- `Services`: infrastructure adapters for Revit API, REST API, sync, export, validation, reporting and settings.
- `Commands`: Revit external commands and ribbon entry points.
- `Views` / `ViewModels`: WPF MVVM presentation layer.
- `Infrastructure`: dependency registry and composition helpers.

## Non-Functional Requirements

- `async/await`: REST, sync, auth and backend communication use async APIs.
- Dependency Injection: service construction is centralized through `Infrastructure/ServiceRegistry`; production can replace it with `Microsoft.Extensions.DependencyInjection`.
- Repository Pattern: `IRepository<TEntity>` abstraction.
- Unit Of Work: `IUnitOfWork` abstraction.
- CQRS: `ICommand`, `ICommandHandler`, `IQuery`, `IQueryHandler`.
- MediatR-ready: command/query handler signatures mirror mediator-style request handling.
- FluentValidation-ready: `IValidator<TModel>` abstraction is present for validation adapters.
- AutoMapper-ready: `IMapper<TSource, TDestination>` abstraction is present for mapping adapters.
- SOLID: features are split into focused services and commands.
- Domain Events: `IDomainEvent`, `IDomainEventDispatcher` and `ProjectSynchronizedEvent`.
- JWT / Refresh Token: `AuthService` supports bearer access tokens and refresh flow.
- Swagger: backend contract is documented in `docs/openapi.yaml`.
- Docker: backend deployment should run through Docker/Docker Compose.
- CI/CD: CI workflow builds the .NET plugin.

## Sync Flow

```text
Revit -> Plugin -> Backend API -> PostgreSQL
                    |
                    -> MinIO export storage
                    -> RabbitMQ async jobs
                    -> Redis cache
                    -> Prometheus/Grafana monitoring
```
