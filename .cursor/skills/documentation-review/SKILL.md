---
name: documentation-review
description: Review and improve README and AI usage documentation for the library system.
---

# Documentation Review

Use this skill when creating or improving README.md and docs/AI_USAGE.md.

## README must include

- project overview and informal user story
- tech stack
- setup instructions
- .cursor file structure and explanation of every rule and skill
- environment variables
- justfile command reference
- how to install dependencies
- how to run backend
- how to run frontend
- how to run the full stack
- how to run with Docker
- how to run tests
- seeded demo credentials
- API examples with curl
- OpenAPI/Swagger location
- error response examples
- loan business rules
- design decisions
- assumptions
- AI usage summary linking to docs/AI_USAGE.md

## Environment documentation

README must document:

Root variables:

~~~env
BACKEND_PORT=8080
FRONTEND_PORT=4200
API_BASE_URL=http://localhost:8080
CORS_ALLOWED_ORIGINS=http://localhost:4200
SWAGGER_ENABLED=true
~~~

Backend variables:

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

Frontend variables:

~~~env
API_BASE_URL=http://localhost:8080
~~~

## AI_USAGE.md must include

- how AI was used
- prompt log
- statement that generated code was reviewed and tested
- final validation commands

## Tone

Keep documentation:

- concise
- professional
- honest
- not defensive
- not overly long

Avoid saying:

- “AI built this”
- “I relied on AI”

Prefer:

- “AI was used to support planning, implementation review, test case generation, and documentation review.”
- “All generated code was manually reviewed, edited, tested, and validated.”

## Output

Return improved documentation or concrete documentation findings.
