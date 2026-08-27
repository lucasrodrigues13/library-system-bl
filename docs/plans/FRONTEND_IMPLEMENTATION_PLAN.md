# Frontend Implementation Plan

This document records the approved frontend plan for the library system.

## Architecture

- **`core/auth`**: token storage, current user, login/logout, guards
- **`core/http`**: JWT interceptor, API error parser
- **`core/api`**: thin HttpClient wrappers
- **`features/login`**: seeded credentials visible on the page
- **`features/books`**: client catalog and admin CRUD
- **`features/users`**: admin CRUD
- **`features/loans`**: create (max 3 titles), insufficient-stock retry, return

## Layout

```text
frontend/src/app/
  core/
  layouts/
  features/login/
  features/books/
  features/users/
  features/loans/
  shared/
```

## UX

- Material admin shell: black header, black sidebar, green actions
- Role-aware sidebar links
- Client: available books only
- Admin: users, books, loans
- List pages in cards with search; green **Add** (`+`) opens a create/edit dialog
- Delete confirmation dialog before API calls
- Loan form: borrower + up to 3 titles, in a dialog
- `INSUFFICIENT_STOCK` lists failing titles in the loan dialog; admin removes them and retries
- Loading, empty, and error states

## Coverage targets

| Area | Target |
|------|--------|
| Guards | unauthenticated redirect, client blocked from admin |
| Error parser | envelope with and without details |
| Loan retry helper | removes only unavailable titles |

## Implementation outcome

Completed: Angular 19 Material app with login, client catalog, admin CRUD in dialogs, delete confirmation, and loan retry UX.

## Out of scope

- NgRx
- Client self-service loans
- OpenAPI codegen
