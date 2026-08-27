---
name: principal-review
description: Review the full repository for correctness, architecture, tests, and documentation quality.
---

# Principal Review

Use this skill before considering work complete.

Review the repository as a production-ready .NET full-stack library system.

## Review dimensions

Check:

1. Feature completeness
   - Angular frontend exists
   - ASP.NET Web API exists
   - MySQL + EF Core exists
   - users can be created and stored
   - login works
   - authorized and anonymous endpoints exist
   - admin vs client access is enforced
   - book CRUD exists
   - atomic loans with stock rules exist
   - Swagger UI is available
   - Docker Compose includes MySQL, API, and UI
   - justfile exists
   - environment examples exist
   - tests exist for domain, application, infrastructure, and API
   - seed data exists
   - README exists
   - AI usage is documented

2. Correctness
   - max 3 titles per loan
   - one unit per title
   - insufficient stock fails the whole loan
   - error details name the unavailable titles
   - return restores stock
   - clients only see available books
   - clients cannot mutate users, books, or loans
   - API and Swagger match

3. Architecture
   - Domain has no EF/HTTP dependencies
   - Application has no controllers or DbContext
   - Infrastructure implements persistence and JWT
   - Api is the composition root
   - frontend isolates API clients from feature UI
   - frontend uses a configurable API base URL
   - no unnecessary infrastructure
   - Docker setup is practical and not overcomplicated

4. .NET quality

   * Keep Clean Architecture pragmatic.
   * Prefer small services over MediatR unless it clearly helps.
   * Keep loan policy in the domain.
   * Use explicit error codes.
   * Cover policy with table-driven or equivalent xUnit theories.
   * Follow KISS: simplest solution that satisfies the business rules.
   * Apply SOLID pragmatically: separate domain, use cases, persistence, and HTTP.

5. Angular quality

   * Use Angular Material for readable layout.
   * Keep feature modules/folders small.
   * Isolate HttpClient in API services.
   * Use functional guards.
   * Show loading, empty, and error states.
   * Implement the loan retry UX (remove unavailable titles, save again).
   * Avoid NgRx and unnecessary abstractions.

6. Documentation
   - setup instructions
   - environment variable documentation
   - justfile commands
   - Docker instructions
   - seeded credentials
   - API examples
   - Swagger details
   - design decisions
   - assumptions
   - AI usage summary

7. Risk
   - anything that could look sloppy
   - anything that could look overengineered
   - anything that could confuse operators or future maintainers
   - anything undocumented in README

## Output format

Return:

~~~md
## Summary

One paragraph.

## Must fix

Concrete issues only.

## Nice to improve

Small improvements if time remains.

## Overengineering check

Anything that should be removed or simplified.

## Missing tests

Specific missing tests.

## Documentation gaps

README or AI usage items that need updating.

## Final recommendation

Ready to ship or not ready.
~~~

Do not suggest new features outside the product scope.
