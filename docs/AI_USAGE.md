# AI Usage

This project used AI assistance as a development aid. All generated code was manually reviewed, edited, tested, and validated.

## How AI was used

AI was used for:

- planning a Clean Architecture full-stack structure for the library system
- translating catalog, member, and loan rules into domain policy and API contracts
- reviewing ASP.NET, EF Core, JWT, and Angular Material structure
- identifying loan edge cases and test cases
- reviewing Docker, justfile, and README completeness

AI was not used as a replacement for final engineering judgment. The solution was kept focused on correctness, maintainability, testability, and a reproducible local setup.

## AI-assisted workflow

I used Cursor with scoped project rules and task-specific skills.

The project rule captured global constraints: Clean Architecture, JWT roles, atomic loans, Docker, justfile, environment-based configuration, and documentation updates.

The skills were used for:

- .NET backend implementation guidance
- Angular frontend implementation guidance
- repository quality review
- documentation review

## Implementation plans

Each major implementation phase has a plan document under [`docs/plans/`](plans/):

| Plan | Document |
|------|----------|
| Backend | [`BACKEND_IMPLEMENTATION_PLAN.md`](plans/BACKEND_IMPLEMENTATION_PLAN.md) |
| Frontend | [`FRONTEND_IMPLEMENTATION_PLAN.md`](plans/FRONTEND_IMPLEMENTATION_PLAN.md) |

## Prompt log

### Prompt 1

```txt
Implement a library management system with .NET, MySQL, EF Core, and Angular.
Use project rules, skills, a justfile, Docker, and a complete README.

Users: admin (full access) and client (available books only).
Books have quantity. Only admins create loans.
Rules: max 3 titles per loan, one unit per title, atomic persist,
INSUFFICIENT_STOCK details so the admin can remove unavailable titles and retry.
Seed enough data for local demos. Everything in English.
```

### Prompt 2

```txt
Create an implementation plan only. Do not write code yet.
Save plan documents under docs/plans/.
```

### Prompt 3

```txt
Implement the approved plan: scaffold, backend with TDD for loan policy,
Angular Material UI, Docker Compose, README and AI_USAGE.
```

### Prompt 4

```txt
Implement UI improvements based on angular-material-admin-full.
Use only black and green. Open add/edit forms as modals from a + button
instead of showing them under the list. Confirm before delete.
```

### Prompt 5

```txt
Remove take-home, interview PDF, and assignment framing from documentation
so the repository reads as a professional library management system.
```

## Final validation

Commands run before considering the implementation complete:

```bash
dotnet test backend/LibrarySystem.sln
npx ng test --watch=false --browsers=ChromeHeadless   # from frontend/
npx ng build --configuration production               # from frontend/
dotnet build backend/LibrarySystem.sln -c Release
```

Results on this machine:

- Backend: 29 tests passed (domain 12, application 8, infrastructure 2, API 7)
- Frontend: 14 tests passed (guards, error parser, loan retry helper, login credentials, environment, confirm dialog, admin add-button)
- Angular production build succeeded
- Dockerfiles and `docker-compose.yml` are included so the full stack can be run locally.
