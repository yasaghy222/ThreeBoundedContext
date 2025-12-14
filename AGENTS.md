Agent Guide (AI-Facing)

Purpose & Scope
- This file is the canonical reference for anyone modifying code in this solution. Follow it whenever you add features, change infrastructure, or adjust documentation.
- The repository implements a microservices sample with three bounded contexts (User, Booking, Finance). Anything that is not required for those services, their shared libraries, or the common deploy stack does not belong here.

Repository Snapshot
- Source lives under `src/` with `Services/` for microservices, `Shared/` for reusable libraries, and `Deploy/` for infrastructure compose fragments and environment files.
- Services: `UserService` (REST + gRPC), `BookingService` (REST + RabbitMQ outbox), `FinanceService` (REST + RabbitMQ consumer).
- Shared libraries power logging (Serilog + Seq), consistent error handling, Swagger, RabbitMQ publishing abstractions, and integration event contracts.
- Infrastructure is provisioned via Docker Compose: SQL Server, RabbitMQ, and Seq share a Docker network named `microservices-network` so every service can reach them by container hostname.
- Service ports and URLs (match `README.md` and compose files):
  - UserService API `http://localhost:5001/swagger`, UserService gRPC `http://localhost:5002`
  - BookingService API `http://localhost:5003/swagger`
  - FinanceService API `http://localhost:5005/swagger`
  - Seq dashboard `http://localhost:5341`, RabbitMQ management `http://localhost:15672`, SQL Server `localhost,1433`.

Global Engineering Conventions
- All projects target `net9.0` with nullable reference types and implicit usings enabled. Update package references to the .NET 9 wave when you add dependencies.
- Service layout is fixed: `<Service>/<Service>.Domain`, `.Application`, `.Infrastructure`, `.API`, plus a root-level Dockerfile and compose file per service.
- Clean architecture rules apply:
  - Domain contains aggregates, entities, value objects, domain events, and repository interfaces.
  - Application hosts MediatR commands/queries, handlers, DTOs, validators, and pipeline behaviors (validation happens via FluentValidation + `ValidationBehavior`).
  - Infrastructure implements repositories (EF Core + SQL Server), messaging, gRPC clients, and background services (outbox processor, RabbitMQ consumer).
  - API projects stay thin: register Application + Infrastructure via their `AddApplication()` / `AddInfrastructure()` extensions, expose controllers, and wire background hosted services from the Infrastructure layer.
- Program.cs pattern for every API:
  - Load configuration from environment variables first, then `appsettings.json`, then `appsettings.{Environment}.json`.
  - Call `builder.AddLoggingExtension("<ServiceName>")`, `services.AddErrorHandling()`, `services.AddApplication()`, `services.AddInfrastructure(configuration)`, `services.AddControllers()`, and `services.AddSwaggerExtension(configuration, environment)`.
  - Register gRPC only where needed (UserService) and expose `/health` via `MapHealthChecks`.
  - Use `UseErrorHandling()` and `UseLoggingExtension()` in the middleware pipeline before routing. Swagger UI is enabled in Development by default; Production exposure is controlled via each service's `Swagger` section.
  - Controllers use `[Route("api/[controller]")]` and PascalCase action names. Return the appropriate status codes (`Created`, `Conflict`, `NotFound`, etc.) as exemplified in the existing controllers.
- Health checks must cover SQL Server and RabbitMQ for every service that depends on them. Connection strings are assembled from the shared `SQLSERVER_*` and `RABBITMQ_*` environment variables to avoid duplication.
- Background services (UserService gRPC host, BookingService OutboxProcessor, FinanceService BookingCreatedConsumer) must be registered through DI so they start automatically when the API boots.
- Architectural patterns pulled from the root README:
  - CQRS via MediatR for every service (commands/queries in Application layer).
  - Domain events bubble up to integration events in Shared.Contracts.
  - Outbox pattern + RabbitMQ ensures consistent async handoffs.
  - Idempotency in FinanceService prevents duplicate invoices.
  - Observability: Serilog + Seq, health checks on `/health`, and correlation IDs in every log entry.

Configuration & Environment
- JSON appsettings store non-sensitive defaults (Swagger metadata, logging levels, feature toggles). Secrets and hostnames live in `.env` files.
- Each service keeps its `.env` under `<Service>.API/.env`; these files are mounted inside the container by the service-specific Compose file and include only the keys the service uses (SQL Server, RabbitMQ, Seq, Kestrel, Swagger toggles).
- Shared infrastructure env files live under `src/Deploy/<Component>/.env` for SQL Server, RabbitMQ, and Seq. Compose files reference them via `env_file` so the same credentials are reused everywhere.
- Common keys:
  - Database: `SQLSERVER_HOST`, `SQLSERVER_PORT`, `SQLSERVER_USER`, `SA_PASSWORD`, `SQLSERVER_DB` (per service).
  - Messaging: `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD`.
  - Logging: `SEQ_URL` plus `Serilog__*` overrides when needed.
  - Hosting: `Kestrel__Endpoints__Http__Url`, `Kestrel__Endpoints__Grpc__Url`, `Kestrel__Endpoints__Grpc__Protocols`, `ASPNETCORE_ENVIRONMENT`.
- `docker-compose.yml` at the repo root includes the deploy fragments and each service compose file. Running `docker compose up --build` spins up everything on the `microservices-network`.
- `start.sh` automates the full lifecycle (cleanup, network creation, compose up, post-start logging). Update it whenever service names, ports, or compose files change.

Shared Libraries & Cross-cutting
- `Shared.Logging` exposes `AddLoggingExtension`/`UseLoggingExtension`. It configures Serilog with Seq sinks, per-request correlation IDs, and enrichers. Do not create ad-hoc Serilog loggers in services; rely on the extension.
- `Shared.ErrorHandling` wires structured validation responses and the `ExceptionHandlingMiddleware`. Do not bypass it with manual try/catch blocks in controllers.
- `Shared.Swagger` centralizes Swagger/Swashbuckle setup (documents, security definition, enum filters, Persian summaries). Populate each service's `Swagger` section through configuration instead of editing Program.cs.
- `Shared.Infrastructure` provides the `IMessagePublisher` abstraction and RabbitMQ implementation with connection reuse, durable exchanges, and JSON serialization.
- `Shared.Contracts` defines the integration events that flow across services: `UserCreatedEvent` and `BookingCreatedEvent`. Any new cross-service contract must be added here and versioned carefully.

Messaging & Data Flow
- SQL Server is the backing store for every service (`UserDb`, `BookingDb`, `FinanceDb`). EF Core migrations live under each Infrastructure project and are applied automatically on startup via `Database.Migrate()`.
- User verification between BookingService and UserService happens synchronously over gRPC (`UserGrpcService.GetUserById`). BookingService may not bypass gRPC when validating users.
- RabbitMQ sits at the center of asynchronous flows:
  - UserService publishes `UserCreatedEvent` domain events to exchange `user-events` with routing key `user.created` through the `DomainEventDispatcher`.
  - BookingService persists integration events to its Outbox table and relies on `OutboxProcessor` (5-second polling interval, three retries) to publish `BookingCreatedEvent` messages to exchange `booking-events` with routing key `booking.created`.
  - FinanceService runs `BookingCreatedConsumer`, which binds queue `finance-booking-created` to the `booking-events` exchange, processes messages with MediatR commands (`CreateInvoiceCommand`), and acknowledges work once invoice creation succeeds. Failed messages are rejected without requeue; add DLQs if reliability requirements change.
- When you add new events, keep exchange names, routing keys, and queue bindings explicit in code and document them in this file and the service README.

Service Blueprints

User Service (ports 5001 HTTP / 5002 gRPC)
- Capabilities: register users, fetch users by id/email/list, and expose `UserGrpcService` so BookingService can validate user IDs synchronously.
- Storage: SQL Server `UserDb`. Database migrations run during startup; on success the API and gRPC endpoints start listening.
- Integrations: publishes `UserCreatedEvent` through RabbitMQ exchange `user-events`. When you introduce new domain events, push them through the same dispatcher so they land in Shared.Contracts.
- API style: `[Route("api/Users")]` with POST (201 + CreatedAt), GET by id/email (404 fallback), GET collection. Stick to the same status codes, validation strategy, and logging pattern (log success and conflicts, but never secrets). Keep the gRPC contract (`protos/users.proto`) up to date when you add new user-query needs.

Booking Service (port 5003)
- Responsibilities: create bookings, fetch bookings, coordinate user validation, and publish booking lifecycle events.
- Dependencies: invokes UserService gRPC to make sure a user exists and is active before persisting, stores data in `BookingDb`, and writes integration events into the Outbox table so publishing is transactionally consistent.
- Outbox pattern: `BookingRepository` writes `OutboxMessage` entries alongside the booking record. `OutboxProcessor` polls every 5 seconds, publishes `BookingCreatedEvent` messages via `IMessagePublisher`, marks records as processed, and retries up to 3 times before leaving diagnostics in the table. This implements the Outbox + Polling Publisher described in `README.md`.
- API expectations: `[Route("api/Bookings")]` exposes POST (creates booking + outbox entry), GET (all bookings), GET by user, plus any future queries. Every request runs through FluentValidation before hitting handlers. Successful booking creation triggers RabbitMQ publishing and eventually Finance invoice creation, matching the “BookingCreated → Invoice” flow documented in the README sequence.

Finance Service (port 5005)
- Responsibilities: listen for `BookingCreatedEvent`, enforce idempotency, and create invoices exposed via REST APIs (`/api/Invoices`, `/api/Invoices/by-booking/{bookingId}`, `/api/Invoices/{id}/pay`, etc.).
- Messaging: `BookingCreatedConsumer` manages the RabbitMQ connection/queue binding and forwards deserialized events to MediatR commands. Always keep the queue durable and specify QoS (prefetch 1) so invoice creation stays sequential and traceable.
- Persistence: SQL Server `FinanceDb` holds invoices and payment state. Handlers must check for existing invoices by booking id before inserting to keep the consumer idempotent.
- API style: controllers follow the same pattern as other services (MediatR, FluentValidation, structured logging). Public endpoints align with the README walkthrough: create invoice via event, retrieve via `/api/invoices/by-booking/{bookingId}`, and expose payment commands under `/api/invoices/{id}/pay`. Add new endpoints only when they align with the finance bounded context.

Testing & Tooling
- Run `dotnet test` at the repository root to execute all unit tests. Add service-specific test projects under `tests/` and hook them into the solution.
- Use the `start.sh` script or `docker compose up --build` to launch the full stack. Confirm health endpoints (`/health`) for each service and check Seq (http://localhost:5341) for structured logs with correlation IDs.
- When you add or change endpoints, update both this guide and the root `README.md` so humans understand how to interact with the system.
