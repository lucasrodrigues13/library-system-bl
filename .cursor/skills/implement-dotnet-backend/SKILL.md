---
name: implement-dotnet-backend
description: Implement or improve the ASP.NET Core Web API, Clean Architecture layers, EF Core MySQL, JWT auth, Swagger, seeds, and tests for the library system.
---

# Implement .NET Backend

Use this skill when implementing or improving the backend.

## Goal

Build a small Clean Architecture ASP.NET Core Web API for a library.

The backend should demonstrate:

- domain rules independent of EF Core and HTTP
- JWT authentication with `Admin` and `Client` roles
- atomic loan creation with stock validation
- environment-based configuration
- Swagger UI
- TDD for loan policy
- seed data for local demos
- simple architecture

## Expected structure

~~~txt
backend/
  LibrarySystem.sln
  .env.example
  Dockerfile
  src/
    LibrarySystem.Domain/
    LibrarySystem.Application/
    LibrarySystem.Infrastructure/
    LibrarySystem.Api/
  tests/
    LibrarySystem.Domain.Tests/
    LibrarySystem.Application.Tests/
    LibrarySystem.Infrastructure.Tests/
    LibrarySystem.Api.Tests/
~~~

## Layer rules

- **Domain**: entities, enums, `LoanPolicy`, `Result`, error codes. No EF, no ASP.NET, no JWT.
- **Application**: use cases, DTOs, repository interfaces, token/password abstractions. No EF DbContext, no controllers.
- **Infrastructure**: EF Core MySQL, JWT, password hashing, seed, repository implementations.
- **Api**: controllers, auth middleware, Swagger, CORS, composition root.

Do not put business rules in controllers.

## Configuration requirements

Required backend variables:

~~~env
ASPNETCORE_URLS=http://localhost:8080
CORS_ALLOWED_ORIGINS=http://localhost:4200
SWAGGER_ENABLED=true
ConnectionStrings__Default=Server=localhost;Port=3306;Database=library;User=library;Password=library;
Jwt__Secret=change-me-to-a-long-development-secret-of-at-least-32-chars
Jwt__Issuer=LibrarySystem
Jwt__Audience=LibrarySystem
Jwt__ExpiryMinutes=480
~~~

Rules:

- Provide `backend/.env.example`.
- Do not require a real `.env` file to run locally if appsettings contain development defaults.
- JWT secret must be configurable.
- CORS allowed origins must be configurable.
- Swagger should be possible to disable through configuration.
- Tests must not require a running MySQL instance (use SQLite or EF InMemory).
- Document all variables in README.md.

## API contract

Anonymous:

- `POST /api/v1/auth/login`
- `GET /health`

Authorized:

- `GET /api/v1/auth/me`
- `GET /api/v1/books` (filter `Quantity > 0` for `Client`)

Admin only:

- `GET|POST /api/v1/users`
- `GET|PUT|DELETE /api/v1/users/{id}`
- `POST|PUT|DELETE /api/v1/books` and `GET /api/v1/books/{id}`
- `GET|POST /api/v1/loans`
- `GET /api/v1/loans/{id}`
- `POST /api/v1/loans/{id}/return`

Error envelope:

~~~json
{
  "error": {
    "code": "INSUFFICIENT_STOCK",
    "message": "Human-readable message.",
    "details": [
      { "bookId": "...", "title": "...", "available": 0 }
    ]
  }
}
~~~

`details` is optional except for `INSUFFICIENT_STOCK`.

## Loan policy (must be in Domain)

- Maximum 3 distinct titles per loan
- One unit per title (reject duplicate book IDs)
- A title may be loaned only when `Quantity >= 1` (`Quantity` is available copies; `TotalQuantity` is owned copies)
- Atomic persist: if any title fails, persist nothing
- Return restores available quantity for each item, never above `TotalQuantity`
- Create loans inside a database transaction and re-read stock inside that transaction
- Use optimistic concurrency (`RowVersion`) on `Book` to reduce oversell races

## Seed requirements

Idempotent seed on startup when the users table is empty (or keyed by known emails):

| Email | Password | Role |
|-------|----------|------|
| admin@library.local | Admin123! | Admin |
| alice@library.local | Alice123! | Client |
| bob@library.local | Bob123! | Client |
| carol@library.local | Carol123! | Client |

Catalog must include titles with stock `1–10`, one title with `Quantity = 0`, one title with `Quantity = 1`, and at least one active loan.

## Tests (TDD for policy)

Domain:

- empty loan
- more than 3 titles
- duplicate title
- missing book
- zero stock
- mixed available/unavailable (failure lists only the insufficient titles)
- happy path with 1–3 titles
- return already-returned loan

Application:

- login success/failure
- client book listing hides zero stock
- mapping of policy failures to error codes
- borrower must be a client

Infrastructure:

- seed is idempotent
- loan transaction does not commit partial items

API:

- login 200 and 401
- anonymous access rejected on authorized endpoints
- client cannot call admin endpoints (403)
- client `GET /api/v1/books` hides zero-stock titles
- admin loan create 400 `INSUFFICIENT_STOCK`
- successful loan decrements quantity
- return restores quantity

## Docker requirements

Create a backend Dockerfile that:

- publishes the API
- exposes 8080
- waits/retries for MySQL on startup
- runs migrations and seed
- does not depend on a local `.env` file

## Validation commands

~~~bash
cd backend && dotnet test LibrarySystem.sln
just test-backend
just run-backend
~~~

Fix failures before stopping.

## Documentation handoff

Do not perform a full README rewrite from this skill.

When backend behavior, configuration, commands, API contract, Docker setup, Swagger, seeds, or assumptions change, leave a concise note of what must be reflected in README.md.

The documentation-review skill is responsible for updating README.md and docs/AI_USAGE.md.

Before stopping, summarize any documentation-impacting changes under:

```md
## Documentation notes

- ...
```
