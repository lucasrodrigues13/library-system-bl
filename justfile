set dotenv-load := true
set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

BACKEND_PORT := env_var_or_default("BACKEND_PORT", "8080")
FRONTEND_PORT := env_var_or_default("FRONTEND_PORT", "4200")
API_BASE_URL := env_var_or_default("API_BASE_URL", "http://localhost:" + BACKEND_PORT)

set export := true

default:
    @just --list

install:
    just install-backend
    just install-frontend

install-backend:
    cd backend; dotnet restore LibrarySystem.sln

install-frontend:
    cd frontend; npm install

debug:
    node scripts/dev.mjs

run:
    just dev

dev:
    docker compose up --build

run-backend:
    cd backend; dotnet watch --project src/LibrarySystem.Api/LibrarySystem.Api.csproj run --urls http://localhost:{{BACKEND_PORT}}

run-frontend:
    cd frontend; npx ng serve --host 0.0.0.0 --port {{FRONTEND_PORT}}

test:
    just test-backend
    just test-frontend

test-backend:
    cd backend; dotnet test LibrarySystem.sln --collect:"XPlat Code Coverage"

test-frontend:
    cd frontend; npx ng test --watch=false --browsers=ChromeHeadless

build:
    just build-backend
    just build-frontend

build-backend:
    cd backend; dotnet build LibrarySystem.sln -c Release

build-frontend:
    cd frontend; npx ng build --configuration production

docker-build:
    docker compose build

docker-build-backend:
    docker build -t library-system-backend ./backend

docker-build-frontend:
    docker build -t library-system-frontend --build-arg API_BASE_URL={{API_BASE_URL}} ./frontend

docker-down:
    docker compose down
