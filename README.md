# EV Charging Point Management System

Master's thesis project @ FER Zagreb. A microservices-based CPMS built around OCPP 1.6 — handles charger communication, RFID authentication, real-time session monitoring & Stripe-powered payments.

**Stack:** .NET (CQRS + Event Sourcing) · React/TypeScript · PostgreSQL · Docker

## Running locally

```bash
cp .env.example .env       # fill in the values
docker compose up -d       # PostgreSQL + pgAdmin

dotnet run --project CPMS.API      # REST API, Swagger at /swagger
dotnet run --project CPMS.Proxy    # OCPP 1.6-J WebSocket gateway

cd CPMS.Frontend && npm install && npm run dev
```
