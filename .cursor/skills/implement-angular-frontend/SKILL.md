---
name: implement-angular-frontend
description: Implement or improve the Angular Material library frontend, JWT auth, role-based routes, loan retry UX, tests, and Docker setup.
---

# Implement Angular Frontend

Use this skill when implementing or improving the frontend.

## Goal

Build a polished but simple Angular frontend that consumes the library API.

The frontend should demonstrate:

- login with seeded credentials visible on the page
- role-based navigation
- client catalog of available books
- admin CRUD for users and books
- admin loan create with insufficient-stock retry
- loading, empty, and error states
- environment-based API URL
- testability

## Stack

Use:

- Angular standalone components
- TypeScript
- Angular Material
- HttpClient
- functional route guards
- unit tests (Karma/Jasmine or equivalent)

Avoid:

- NgRx, Akita, or other global stores
- server-side rendering
- generating a full OpenAPI client unless it clearly helps
- copying extra admin-template pages (charts, theme switcher, chat, e-commerce)

Do apply a visible Material admin layout: black header, black sidebar, green primary actions, list cards, and dialogs. A gray list with an inline form is not enough.

## Expected structure

~~~txt
frontend/
  .env.example
  Dockerfile
  nginx.conf
  src/app/
    core/          auth, interceptor, config, API clients
    layouts/       public vs authenticated shell
    features/login
    features/books
    features/users
    features/loans
    shared/
~~~

## Configuration requirements

~~~env
API_BASE_URL=http://localhost:8080
~~~

Rules:

- Provide `frontend/.env.example`.
- Do not commit real `.env` files.
- Do not hardcode the backend URL inside feature components.
- Keep configuration in a small environment module.
- For Docker, pass `API_BASE_URL` as a build argument.
- Document all variables in README.md.

## UX direction

- Login page shows seeded demo accounts (admin and at least one client).
- Unauthenticated users are redirected to login.
- `Client` cannot open admin routes.
- Admin can manage users, books, and loans.
- Clients see a catalog of books with `quantity > 0` only (backend-enforced; still do not offer admin actions).
- Loan form: select a client borrower and up to 3 titles.
- On `INSUFFICIENT_STOCK`, show which titles failed, let the admin remove them from the selection, and submit again.
- Use a Material admin shell: header + sidebar, role-aware nav, tables in cards.
- Create and edit through dialogs opened from a green **Add** (`+`) button. Do not leave forms always visible under the list.
- Confirm delete (and loan return) in a dialog before calling the API.
- Palette is black and green only. Primary buttons, active nav, and chips must actually look green.

## API behavior

Call the backend. Do not invent local persistence.

Loan create request:

~~~json
{
  "borrowerId": "...",
  "bookIds": ["...", "..."]
}
~~~

Parse the stable error envelope and display `error.message`. For `INSUFFICIENT_STOCK`, map `error.details` onto the selected title list.

## State management

Keep state local to feature components or small services.

An `AuthService` holding the current user and token in `localStorage` or `sessionStorage` is expected. Do not introduce NgRx.

## Required tests

Tests should cover:

- login component renders seeded credential hint
- auth guard redirects unauthenticated users
- admin guard blocks `Client`
- API error envelope parsing
- loan retry helper removes unavailable titles and keeps available ones
- configurable API base URL if practical

## Docker requirements

Create a frontend Dockerfile that:

- builds the Angular production bundle
- serves it with nginx
- uses `API_BASE_URL` at build time
- runs without local Node installed

## Validation commands

~~~bash
cd frontend && npx ng test --watch=false --browsers=ChromeHeadless
cd frontend && npx ng build --configuration production
just test-frontend
just run-frontend
~~~

Fix failures before stopping.

## Documentation handoff

Do not perform a full README rewrite from this skill.

When frontend behavior, configuration, commands, Docker setup, API integration, UI behavior, or assumptions change, leave a concise note of what must be reflected in README.md.

The documentation-review skill is responsible for updating README.md and docs/AI_USAGE.md.

Before stopping, summarize any documentation-impacting changes under:

```md
## Documentation notes

- ...
```
