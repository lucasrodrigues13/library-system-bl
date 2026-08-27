# .NET Backend Implementation Plan

This document records the approved backend plan for the library system.

## Architecture

- **`LibrarySystem.Domain`**: entities, enums, `LoanPolicy`, `Result`, error catalog
- **`LibrarySystem.Application`**: use cases, DTOs, repository and token abstractions
- **`LibrarySystem.Infrastructure`**: EF Core MySQL, JWT, password hashing, seed
- **`LibrarySystem.Api`**: HTTP, CORS, Swagger, JWT bearer, composition root

## Layout

```text
backend/
  LibrarySystem.sln
  Dockerfile
  src/LibrarySystem.Domain/
  src/LibrarySystem.Application/
  src/LibrarySystem.Infrastructure/
  src/LibrarySystem.Api/
  tests/LibrarySystem.Domain.Tests/
  tests/LibrarySystem.Application.Tests/
  tests/LibrarySystem.Infrastructure.Tests/
  tests/LibrarySystem.Api.Tests/
```

## API

- `POST /api/v1/auth/login` — anonymous
- `GET /api/v1/auth/me` — authorized
- `GET /health` — anonymous
- Users, books, loans as documented in the project rule
- Swagger UI at `/swagger` when enabled

## Error codes

| Code | HTTP | When |
|------|------|------|
| `INVALID_INPUT` | 400 | Validation failures |
| `LOAN_EMPTY` | 400 | No titles selected |
| `LOAN_TOO_MANY_TITLES` | 400 | More than 3 titles |
| `LOAN_DUPLICATE_TITLE` | 400 | Duplicate book id |
| `INSUFFICIENT_STOCK` | 400 | One or more titles have no remaining copies |
| `DUPLICATE_EMAIL` | 400 | Email already registered |
| `DUPLICATE_ISBN` | 400 | ISBN already registered |
| `INVALID_CREDENTIALS` | 401 | Bad login |
| `UNAUTHORIZED` | 401 | Missing/invalid token |
| `FORBIDDEN` | 403 | Wrong role |
| `NOT_FOUND` | 404 | Missing entity |
| `LOAN_ALREADY_RETURNED` | 400 | Return called twice |
| `USER_HAS_ACTIVE_LOANS` | 400 | Delete blocked |
| `BOOK_HAS_ACTIVE_LOANS` | 400 | Delete blocked |

## Coverage targets

| Area | Target |
|------|--------|
| Domain loan policy | 100% of branches in `LoanPolicy` |
| Application services | core success and failure paths |
| API | auth, role, stock, and loan retry contract |

## Implementation outcome

Completed: Clean Architecture solution targeting `net9.0`, JWT auth, EF Core MySQL with migrations including `TotalQuantity` vs available `Quantity`, idempotent seed data, and passing tests that do not require a running MySQL instance.

## Out of scope

- Frontend
- Full README rewrite (documentation-review skill)
