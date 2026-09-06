# EV Charging Point Management System

Master's thesis project @ FER Zagreb. A microservices-based CPMS built around OCPP 1.6 — handles charger communication, RFID authentication, real-time session monitoring & Stripe-powered payments.

**Stack:** .NET (CQRS + Event Sourcing) · React/TypeScript · PostgreSQL · Docker

## Running locally

```bash
docker compose up -d       # PostgreSQL + pgAdmin

dotnet run --project CPMS.API      # REST API, Swagger at /swagger
dotnet run --project CPMS.Proxy    # OCPP 1.6-J WebSocket gateway

cd CPMS.Frontend && npm install && npm run dev
```

The compose defaults match `CPMS.API/appsettings.Development.json`, so this works without a
`.env`. Copy `.env.example` to `.env` only when you need Stripe or Mapbox keys, or when you
want to change a port or a password — the file explains which process reads it.

Marten builds its schema on the first connect, so there is no migration step.

## Running from Rider

The solution ships run configurations in `.run/`. Rider picks them up automatically. Run
`cp .env.example .env` once first: the two .NET configurations load that file, and they fail if
it is missing.

| Configuration | Starts | Notes |
|---------------|--------|-------|
| `Infra` | PostgreSQL + pgAdmin | `docker-compose.yml`. Pick your Docker connection if the dropdown is empty. |
| `API` | `CPMS.API` on 5023 | Swagger at `/swagger`, health at `/health`. |
| `Proxy` | `CPMS.Proxy` on 5000 | Chargers connect to `ws://localhost:5000/OCPP/{chargePointId}`. |
| `Frontend` | Vite on 5173 | Proxies `/api` to 5023. |
| `All` | API + Proxy + Frontend | Start `Infra` once first. |

Both .NET configurations load `.env` if it exists, so a Stripe key or an overridden connection
string is picked up without touching `appsettings`.

### Ports

| Port | Service |
|------|---------|
| 5023 | API |
| 5000 | OCPP gateway |
| 5173 | Frontend (Vite) |
| 5432 | PostgreSQL |
| 5050 | pgAdmin |

If another container already holds 5432, either stop it or set `POSTGRES_PORT` and
`ConnectionStrings__MartenDb` in `.env`.
