# Three Bounded Context - Microservices Architecture

A clean architecture microservices solution demonstrating Domain-Driven Design (DDD) principles with three bounded contexts.

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              API Gateway / Client                                │
└─────────────────────────────────────────────────────────────────────────────────┘
                    │                    │                    │
                    ▼                    ▼                    ▼
┌─────────────────────────┐  ┌─────────────────────────┐  ┌─────────────────────────┐
│      User Service       │  │    Booking Service      │  │    Finance Service      │
│    (Port: 5001/5002)    │  │      (Port: 5003)       │  │      (Port: 5005)       │
│                         │  │                         │  │                         │
│  • POST /api/users      │  │  • POST /api/bookings   │  │  • GET /api/invoices    │
│  • GET /api/users/{id}  │  │  • GET /api/bookings    │  │  • POST /invoices/{id}/ │
│  • gRPC: GetUserById    │  │                         │  │         pay             │
│                         │  │                         │  │                         │
│  ┌───────────────────┐  │  │  ┌───────────────────┐  │  │  ┌───────────────────┐  │
│  │     UserDb        │  │  │  │    BookingDb      │  │  │  │    FinanceDb      │  │
│  │   (SQL Server)    │  │  │  │   (SQL Server)    │  │  │  │   (SQL Server)    │  │
│  └───────────────────┘  │  │  └───────────────────┘  │  │  └───────────────────┘  │
└────────────┬────────────┘  └───────────┬────────────┘  └────────────┬────────────┘
             │                           │                             │
             │    ┌──────────────────────┼─────────────────────────────┘
             │    │ gRPC (sync)          │ RabbitMQ (async)
             │    ▼                      ▼
             │   Booking validates      Booking publishes
             │   user via gRPC          BookingCreated event
             │                           │
             │                           ▼
             │                    ┌─────────────────┐
             └───────────────────►│    RabbitMQ     │◄────────────────────
                                  │  Message Broker │
                                  └─────────────────┘
                                           │
                                           ▼
                                  Finance consumes
                                  BookingCreated → Creates Invoice
```

## 🌟 Key Features

### Clean Architecture (per service)
- **Domain Layer**: Entities, Value Objects, Domain Events, Repository Interfaces
- **Application Layer**: CQRS (Commands/Queries), Handlers, DTOs, Validators
- **Infrastructure Layer**: EF Core, RabbitMQ, gRPC, Outbox Pattern
- **Presentation Layer**: REST APIs, Health Checks, Swagger

### Communication Patterns
- **Synchronous (gRPC)**: Booking → User (validate user exists & is active)
- **Asynchronous (RabbitMQ)**: Booking → Finance (create invoice after booking)

### Advanced Patterns
- ✅ **Outbox Pattern**: Ensures reliable message delivery (BookingService)
- ✅ **Idempotency**: Prevents duplicate invoice creation (FinanceService)
- ✅ **CQRS**: Command/Query separation with MediatR
- ✅ **Domain Events**: Clean event-driven architecture

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 9 SDK (for local development)

### Run with Docker Compose

```bash
# Clone and navigate to the project
cd ThreeBoundedContext

# Start all services
docker-compose up --build

# Stop services
docker-compose down
```

### Services URLs

| Service | Port | URL |
|---------|------|-----|
| User Service API | 5001 | http://localhost:5001/swagger |
| User Service gRPC | 5002 | http://localhost:5002 |
| Booking Service API | 5003 | http://localhost:5003/swagger |
| Finance Service API | 5005 | http://localhost:5005/swagger |
| RabbitMQ Management | 15672 | http://localhost:15672 (guest/guest) |
| SQL Server | 1433 | localhost,1433 (sa/YourStrong@Passw0rd) |

### Health Checks
Each service exposes a health endpoint:
- http://localhost:5001/health
- http://localhost:5003/health
- http://localhost:5005/health

## 📝 API Usage

### 1. Register a User (UserService)

```bash
curl -X POST http://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "fullName": "John Doe",
    "password": "secret123"
  }'
```

Response:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john@example.com",
  "fullName": "John Doe"
}
```

### 2. Create a Booking (BookingService)

```bash
curl -X POST http://localhost:5003/api/bookings \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "description": "Hotel Reservation",
    "amount": 250.00,
    "bookingDate": "2024-12-25T14:00:00Z"
  }'
```

This will:
1. ✅ Validate user via gRPC (sync)
2. ✅ Create booking in database
3. ✅ Save event to Outbox table (same transaction)
4. ✅ Background worker publishes to RabbitMQ
5. ✅ Finance service receives and creates Invoice

### 3. Check Invoice (FinanceService)

```bash
# Get invoice by booking ID
curl http://localhost:5005/api/invoices/by-booking/{bookingId}
```

## 🏛️ Project Structure

```
ThreeBoundedContext/
├── src/
│   ├── Services/
│   │   ├── UserService/
│   │   │   ├── UserService.Domain/        # Entities, ValueObjects, Events
│   │   │   ├── UserService.Application/   # Commands, Queries, DTOs
│   │   │   ├── UserService.Infrastructure/# EF Core, RabbitMQ, gRPC
│   │   │   ├── UserService.API/           # Controllers, Program.cs
│   │   │   └── Dockerfile
│   │   ├── BookingService/
│   │   │   ├── BookingService.Domain/
│   │   │   ├── BookingService.Application/
│   │   │   ├── BookingService.Infrastructure/ # + Outbox Pattern
│   │   │   ├── BookingService.API/
│   │   │   └── Dockerfile
│   │   └── FinanceService/
│   │       ├── FinanceService.Domain/
│   │       ├── FinanceService.Application/    # + Idempotency
│   │       ├── FinanceService.Infrastructure/ # RabbitMQ Consumer
│   │       ├── FinanceService.API/
│   │       └── Dockerfile
│   └── Shared/
│       ├── Shared.Contracts/              # Integration Events
│       └── Shared.Infrastructure/         # RabbitMQ Publisher
├── tests/
├── docker-compose.yml
└── README.md
```

## 💡 Trade-offs & Design Decisions

### Why gRPC for User Validation?
- **Sync required**: Booking needs immediate user validation
- **Performance**: gRPC is faster than REST for internal communication
- **Type safety**: Proto files ensure contract consistency

### Why RabbitMQ for Invoice Creation?
- **Async is acceptable**: Invoice can be created slightly later
- **Reliability**: Message queue ensures no lost events
- **Decoupling**: Finance service can be down temporarily

### Why Outbox Pattern?
- **Atomicity**: Event is saved in same transaction as booking
- **No lost events**: Even if RabbitMQ is down, events are persisted
- **Eventual consistency**: Background worker retries failed messages

### Why Idempotency in Finance?
- **Duplicate prevention**: RabbitMQ may deliver same message twice
- **At-least-once delivery**: Trade reliability for potential duplicates
- **Simple check**: `ExistsByBookingId` before creating invoice

## 🔧 Local Development

```bash
# Restore dependencies
dotnet restore

# Run SQL Server and RabbitMQ
docker-compose up sqlserver rabbitmq -d

# Run each service (in separate terminals)
cd src/Services/UserService/UserService.API && dotnet run
cd src/Services/BookingService/BookingService.API && dotnet run
cd src/Services/FinanceService/FinanceService.API && dotnet run
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific service tests
dotnet test tests/UserService.Tests
```

## 📊 Observability

### Structured Logging (Serilog)
Each service uses Serilog with console output:
```
[INF] User registered successfully: {UserId}
[INF] Created invoice {InvoiceId} for booking {BookingId}
```

### Health Checks
- SQL Server connectivity
- RabbitMQ connectivity

## 📜 License

MIT License

---

Built with ❤️ for microservices architecture interviews
