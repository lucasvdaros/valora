# Valora API

ASP.NET Core 10 API for the local Valora application. The API listens only on loopback by default and uses PostgreSQL.

## Local setup

1. Copy `.env.example` to `.env` at the repository root and set a local PostgreSQL password.
2. Start PostgreSQL with `docker compose up -d postgres`.
3. Set `ConnectionStrings__Valora` in the shell to a connection string using the same password as `.env`. Do not commit real credentials.
4. From the repository root, run `dotnet tool restore` and `dotnet tool run dotnet-ef database update --project src/Valora.Infrastructure --startup-project src/Valora.Api`.
5. Run `dotnet run --project src/Valora.Api --urls http://127.0.0.1:5080`.

The default time zone is `America/Sao_Paulo`; override it with `Valora__TimeZone`. The server's bind address should remain loopback for the unauthenticated V1. Do not expose this API to a LAN or public network without adding authentication and reviewing CORS/security settings.

In Development, Scalar's interactive API reference is available at `/scalar` and the OpenAPI JSON at `/openapi/v1.json`. API routes use `/api/v1`.

Service registration is separated by layer: `AddValoraApi`, `AddValoraApplication`, `AddValoraDomain` and `AddValoraInfrastructure`. Endpoint mapping and HTTP middleware live in API extension methods; `Program.cs` is the composition root. The Domain extension intentionally adds no services because domain rules remain container independent.

## Configuration

- `ConnectionStrings__Valora`: PostgreSQL connection string.
- `Valora__TimeZone`: time-zone ID used to determine the current month. Defaults to `America/Sao_Paulo`.
- `ASPNETCORE_URLS`: bind address. Use `http://127.0.0.1:5080` for local-only access.

## Tests

Run `dotnet test Valora.sln`. EF Core InMemory tests cover domain/service behavior; database constraints, migrations, transaction behavior, and PostgreSQL concurrency require integration tests against PostgreSQL.
