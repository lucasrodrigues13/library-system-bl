# Library System

A full-stack library management system with an ASP.NET Core Web API and an Angular frontend. Librarians register members and titles and lend up to three available titles per member in one atomic loan. Members only see books that still have remaining copies.

## Informal user story

As a librarian, I register members and titles and lend up to three available titles per member in one loan, so the library never over-commits stock. As a member, I only see books I can actually borrow right now.

## Tech stack

| Layer | Stack |
|-------|--------|
| Backend | .NET 9, ASP.NET Core Web API, EF Core, Pomelo MySQL, JWT, Swagger, xUnit |
| Frontend | Angular 19, TypeScript, Angular Material, Karma/Jasmine |
| Tooling | Docker, docker-compose, [just](https://github.com/casey/just) |

## Project structure

```text
backend/          .NET Clean Architecture solution
frontend/         Angular Material UI
docs/             AI usage and implementation plans
docs/plans/       Backend and frontend implementation plans
.cursor/          Cursor rules and skills for agentic development
docker-compose.yml
justfile
.env.example
```

## Cursor configuration

### Rules

| File | Purpose |
|------|---------|
| [`.cursor/rules/project.mdc`](.cursor/rules/project.mdc) | Global project standards: Clean Architecture, JWT roles, loan rules, Docker, justfile, env config, README expectations, scope limits. |

### Skills

| Skill | Purpose |
|-------|---------|
| [`implement-dotnet-backend`](.cursor/skills/implement-dotnet-backend/SKILL.md) | API structure, EF/MySQL, JWT, atomic loans, Swagger, tests, Dockerfile. |
| [`implement-angular-frontend`](.cursor/skills/implement-angular-frontend/SKILL.md) | Angular Material UI, role-based routes, loan retry UX, tests, Docker. |
| [`principal-review`](.cursor/skills/principal-review/SKILL.md) | Quality gate across correctness, architecture, tests, and documentation. |
| [`documentation-review`](.cursor/skills/documentation-review/SKILL.md) | README and `docs/AI_USAGE.md` completeness and accuracy. |

## Prerequisites

- .NET 9 SDK
- Node.js 20+ and npm
- MySQL 8 (for local API runs) or Docker Compose
- [just](https://github.com/casey/just) (recommended command runner)
- Docker (optional, for containerized full stack)
- Chrome or Microsoft Edge (frontend unit tests use headless Chromium)

## Environment variables

Copy example files and adjust as needed. Do not commit real `.env` files.

**Root** [`.env.example`](.env.example) (used by `just` with `dotenv-load`):

```env
BACKEND_PORT=8080
FRONTEND_PORT=4200
API_BASE_URL=http://localhost:8080
CORS_ALLOWED_ORIGINS=http://localhost:4200
SWAGGER_ENABLED=true
APP_ENV=development
MYSQL_ROOT_PASSWORD=libraryroot
MYSQL_DATABASE=library
MYSQL_USER=library
MYSQL_PASSWORD=library
MYSQL_PORT=3306
ConnectionStrings__Default=Server=localhost;Port=3306;Database=library;User=library;Password=library;
Jwt__Secret=change-me-to-a-long-development-secret-of-at-least-32-chars
Jwt__Issuer=LibrarySystem
Jwt__Audience=LibrarySystem
Jwt__ExpiryMinutes=480
```

**Backend** [`backend/.env.example`](backend/.env.example):

```env
ASPNETCORE_URLS=http://localhost:8080
CORS_ALLOWED_ORIGINS=http://localhost:4200
SWAGGER_ENABLED=true
ConnectionStrings__Default=Server=localhost;Port=3306;Database=library;User=library;Password=library;
Jwt__Secret=change-me-to-a-long-development-secret-of-at-least-32-chars
Jwt__Issuer=LibrarySystem
Jwt__Audience=LibrarySystem
Jwt__ExpiryMinutes=480
```

**Frontend** [`frontend/.env.example`](frontend/.env.example):

```env
API_BASE_URL=http://localhost:8080
```

Local Angular development reads [`frontend/src/environments/environment.ts`](frontend/src/environments/environment.ts). Docker replaces the production placeholder at image build time.

## Seeded demo credentials

| Email | Password | Role |
|-------|----------|------|
| `admin@library.local` | `Admin123!` | Admin |
| `alice@library.local` | `Alice123!` | Client |
| `bob@library.local` | `Bob123!` | Client |
| `carol@library.local` | `Carol123!` | Client |

The catalog includes titles with total units `0–10`, one title with `Quantity = 0` (`Out of Print Tales`), one title with `Quantity = 1` (`Dune`), and one active loan for Alice (`The Hobbit`: 5 total, 4 available).

Startup migrates the database (or creates the schema when no migrations apply) and seeds these records only when the users table is empty.

## Install

```bash
just install
```

Backend only:

```bash
just install-backend
```

Frontend only:

```bash
just install-frontend
```

## Run locally

MySQL must be reachable at `ConnectionStrings__Default` (Compose MySQL or a local server).

Start backend + frontend in **one terminal** with prefixed logs and hot reload:

```bash
just debug
```

Or run each service manually in separate terminals.

Start the API (terminal 1):

```bash
just run-backend
```

Start the UI (terminal 2):

```bash
just run-frontend
```

Open http://localhost:4200 (default). The UI calls the API at `API_BASE_URL`.

## Run with Docker

Full stack (MySQL + API + nginx UI):

```bash
just dev
```

- API: http://localhost:8080 (default `BACKEND_PORT`)
- UI: http://localhost:4200 (default `FRONTEND_PORT`)
- Swagger: http://localhost:8080/swagger

Stop services:

```bash
just docker-down
```

Build images without starting:

```bash
just docker-build
```

## Tests and build

```bash
just test
just build
```

Backend or frontend only: `just test-backend`, `just test-frontend`.

On Windows without Google Chrome, frontend tests use Microsoft Edge through `frontend/karma.conf.js`.

## Justfile reference

| Command | Description |
|---------|-------------|
| `just install` | Backend restore + frontend npm packages |
| `just install-backend` | `dotnet restore` |
| `just install-frontend` | `npm install` in `frontend/` |
| `just run-backend` | `dotnet watch` for the API |
| `just run-frontend` | Angular dev server on `FRONTEND_PORT` |
| `just debug` | Backend + frontend in one terminal with prefixed logs |
| `just dev` | `docker compose up --build` (full stack) |
| `just test` | Backend and frontend tests |
| `just test-backend` | `dotnet test` with coverage collector |
| `just test-frontend` | Karma ChromeHeadless |
| `just build` | Release backend build + Angular production build |
| `just docker-build` | Build compose images |
| `just docker-down` | Stop compose services |

On Windows, the justfile uses PowerShell (`;` command chaining).

## API

Anonymous:

- `POST /api/v1/auth/login`
- `GET /health`

Authorized:

- `GET /api/v1/auth/me`
- `GET /api/v1/books` (clients only receive titles with `quantity > 0`)

Admin:

- `GET|POST /api/v1/users`, `GET|PUT|DELETE /api/v1/users/{id}`
- `POST|PUT|DELETE /api/v1/books`
- `GET|POST /api/v1/loans`, `GET /api/v1/loans/{id}`, `POST /api/v1/loans/{id}/return`

### Login

```bash
curl -s -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@library.local","password":"Admin123!"}'
```

### Create loan (atomic)

```bash
TOKEN=replace-with-jwt

curl -s -X POST http://localhost:8080/api/v1/loans \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"borrowerId":"33333333-3333-3333-3333-333333333333","bookIds":["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4","aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"]}'
```

That example pairs `Dune` (1 copy) with `Out of Print Tales` (0 copies) and returns `INSUFFICIENT_STOCK` without writing a loan.

### Error envelope

```json
{
  "error": {
    "code": "INSUFFICIENT_STOCK",
    "message": "One or more titles do not have enough available copies for this loan.",
    "details": [
      {
        "bookId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5",
        "title": "Out of Print Tales",
        "available": 0
      }
    ]
  }
}
```

### Error codes

| Code | HTTP | When |
|------|------|------|
| `INVALID_INPUT` | 400 | Validation failures |
| `LOAN_EMPTY` | 400 | No titles selected |
| `LOAN_TOO_MANY_TITLES` | 400 | More than 3 titles |
| `LOAN_DUPLICATE_TITLE` | 400 | Duplicate book id |
| `INSUFFICIENT_STOCK` | 400 | One or more titles have no remaining copies |
| `DUPLICATE_EMAIL` | 400 | Email already registered |
| `DUPLICATE_ISBN` | 400 | ISBN already registered |
| `LOAN_ALREADY_RETURNED` | 400 | Return called twice |
| `USER_HAS_ACTIVE_LOANS` | 400 | Delete blocked |
| `BOOK_HAS_ACTIVE_LOANS` | 400 | Delete blocked |
| `INVALID_CREDENTIALS` | 401 | Bad login |
| `UNAUTHORIZED` | 401 | Missing or invalid token |
| `FORBIDDEN` | 403 | Wrong role |
| `NOT_FOUND` | 404 | Missing entity |

## Frontend behavior

- Login page documents seeded admin and client credentials.
- Authenticated screens use a Material admin shell: black header, black sidebar, green primary actions.
- Lists sit in cards with search. Create and edit forms open in dialogs from a green **Add** (`+`) button, not as always-visible forms under the table.
- Delete (and loan return) asks for confirmation in a dialog before calling the API.
- Clients see available books only and cannot open Users or Loans.
- Admins manage users, books, and loans. Book catalog shows total owned units and available copies.
- Loan form: pick a client borrower and up to 3 titles. Titles show available vs total.
- On `INSUFFICIENT_STOCK`, the loan dialog lists failing titles. The admin can remove them and save again. Nothing is stored until every remaining title is available.
- Returning a loan restores stock.

## OpenAPI and Swagger

| Resource | URL (server running) |
|----------|----------------------|
| Swagger UI | http://localhost:8080/swagger (when `SWAGGER_ENABLED=true`) |
| OpenAPI JSON | http://localhost:8080/swagger/v1/swagger.json |

## Design decisions

### Backend

- **Clean Architecture without extra frameworks** — Domain, Application, Infrastructure, and Api are separate projects. Use cases are plain services rather than MediatR, which would add ceremony without improving the loan workflow.
- **Custom user + JWT instead of ASP.NET Identity UI** — Stored users, login, and authorized vs anonymous endpoints are covered by a `User` entity, `PasswordHasher<User>`, and JWT. That keeps authentication small and easy to test.
- **Web API + Angular** — The API is the composition root. The UI is a separate SPA so librarians and members can work against a versioned HTTP contract.
- **Loan policy lives in the domain** — Max 3 titles, one unit per title, and all-or-nothing stock checks are independent of EF and HTTP.
- **Atomic persist** — Loan create runs in a transaction. If any title is unavailable, the unit of work rolls back. `Book.RowVersion` reduces oversell races.
- **Total vs available stock** — `TotalQuantity` is how many copies the library owns. `Quantity` is copies on the shelf. Loans decrement available stock; returns increment it without exceeding the total.
- **SQLite in tests, MySQL in runtime** — Tests do not require a running MySQL instance.

### Frontend

- **Role-aware shell** — Functional guards keep clients off admin routes. The backend still enforces authorization.
- **Loan retry UX** — `removeUnavailableTitles` is a pure helper so the admin can drop failed titles and submit again, matching the business rule.
- **Angular Material admin shell** — Header, sidebar, table cards, and dialogs follow a Material admin layout. The palette is black and green. Create/edit forms open from a green Add button instead of sitting under the list.
- **API client isolation** — Feature components call small HttpClient services. JWT is attached by an interceptor.

## Assumptions

### Product scope

- This system covers catalog, member, and loan operations for a small library.
- Clients do not request loans; only admins register loans.
- Fines, due dates, renewals, waitlists, and reservations are out of scope.
- `TotalQuantity` is owned copies. `Quantity` is current available stock and is the value used for loan checks.

### Backend

- Emails are stored lowercase and must be unique.
- Passwords must be at least 8 characters on create.
- Borrowers must have the `Client` role.
- CORS is an allow-list.
- Swagger can be disabled through configuration.

### Frontend

- Session token is stored in `sessionStorage`.
- The production API URL is injected at Docker build time.
- Karma uses ChromeHeadless, falling back to Edge on this Windows machine when Chrome is not installed.

## AI usage

AI was used to support planning, implementation review, test case discovery, and documentation review.

All AI-assisted code was manually reviewed, edited where needed, tested, and validated.

Details: [`docs/AI_USAGE.md`](docs/AI_USAGE.md)
