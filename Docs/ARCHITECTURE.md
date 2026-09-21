# Architecture

ASP.NET Core 10 (.NET 10) vertical-slice API. One process, one assembly, PostgreSQL via EF Core. Features own their HTTP contract, validation, and handler; they share domain entities and infrastructure, not a layered application service stack.

This file is the source of truth for how the system is shaped. Agents update it whenever architecture-relevant code changes (see `.cursor/rules/architecture-doc.mdc`).

## Runtime shape

```
HTTP  →  api/v{version}  →  IEndpoint slice  →  AppDbContext  →  PostgreSQL
                │
                └── OpenAPI per version  →  Scalar UI
```

`Program.cs` registers versioning, OpenAPI, FluentValidation, endpoint discovery, Npgsql, and problem details. After mapping endpoints it calls `Database.EnsureCreated()` — that creates missing tables, it does not evolve them. Migrations are the upgrade path once the model needs to change.

HTTPS redirection is skipped when `DOTNET_RUNNING_IN_CONTAINER=true`. Production uses the exception handler + HSTS; all environments add cache/CSP/frame/nosniff/referrer headers and RFC 7807 status-code pages.

EF uses `ConnectionStrings:DefaultConnection` via `IConfiguration.GetConnectionString`. Local Rider loads it from `appsettings.Development.json` (override with user secrets). The Compose app service sets `ConnectionStrings__DefaultConnection` with host `db`. `.env` is only for the Postgres image (`POSTGRES_USER` / `POSTGRES_PASSWORD` / `POSTGRES_DB`); the API does not parse it.

## Layout

| Path | Role |
|------|------|
| `Program.cs` | Composition root |
| `Src/Features/{Area}/{UseCase}.cs` | One static class per use case: nested request/response records, optional FluentValidation validator, `IEndpoint` |
| `Src/Domain/Entities` | EF-mapped aggregates (`Customer`, `Address`, `Product`, `Order`) plus junctions (`OrderProduct`, `OrderAddress`) |
| `Src/Infrastructure/Databases` | `AppDbContext`, `BaseEntityInterceptor` |
| `Src/Infrastructure/MinimalAPIReflection` | Discovers and maps `IEndpoint` implementations |
| `Src/Infrastructure/OpenApi` | Schema reference ids so nested `Request`/`Response` types do not collide |
| `Src/Infrastructure/Services` | Placeholder Azure types (`AzureMailService`, `AzureKeyVaultService`) — not wired |
| `Src/Infrastructure/Logging` | Empty placeholder folder |
| `Src/Features/Orders/Shared` | Empty placeholder folder |
| `Tools/` | File-based check apps, excluded from the web project compile |
| `Docs/` | This document |
| `docker-compose.yml`, `Dockerfile`, `.env` | Postgres image env + optional published app image |

## Vertical slices

Every concrete `IEndpoint` in the assembly is registered as transient and mapped onto the versioned group `api/v{version:apiVersion}`. Current version set: **v1**.

Convention for a write slice (`CreateCustomer`, `CreateProduct`, `CreateOrder`):

1. Nested `Request` / `Response` records (and nested request types as needed).
2. `AbstractValidator<Request>` when there is a body.
3. `class Endpoint : IEndpoint` with `MapPost`/`MapGet`/`MapDelete` + `WithTags`.
4. Handler takes `AppDbContext`, binds the body/route, validates, talks to EF, returns `IResult`.

Convention for a read slice that returns a graph: **project to nested response records**. Do not serialize EF entities. Navigations (`Address.Customer`, `OrderProduct.Product.OrderProducts`) form cycles; System.Text.Json will recurse until the response 500s mid-stream.

List endpoints page with `page` (min 1) and `pageSize` (1–100), default 1 / 20, newest `CreatedAt` first.

### HTTP surface (v1)

| Method | Path | Slice |
|--------|------|--------|
| `POST` | `/api/v1/customers` | `CreateCustomer` |
| `GET` | `/api/v1/customers` | `GetAllCustomers` |
| `POST` | `/api/v1/products` | `CreateProduct` |
| `POST` | `/api/v1/customers/{customerId}/orders` | `CreateOrder` |
| `GET` | `/api/v1/customers/{customerId}/orders` | `GetAllOrdersFromCustomer` |
| `DELETE` | `/api/v1/customers/{customerId}/orders/{orderId}` | `CancelOrder` — **stub**, returns a new Guid, does not touch the database |

Tags: Customers, Products, Orders.

## Domain and persistence

All persisted types inherit `BaseEntity` (`CreatedAt`, `UpdatedAt`, `DeletedAt`). `BaseEntityInterceptor` stamps those on save and converts `Deleted` into a soft delete (`DeletedAt` set, state flipped to `Modified`).

`AppDbContext` applies a global query filter `DeletedAt == null` on every entity. Use `IgnoreQueryFilters()` when a query must see deleted rows.

Notable mappings:

- `Customer` 1–n `Address`.
- `Order` has required `BillingAddress` and `DeliveryAddress` (`OrderAddress` snapshots, restrict delete), plus n–n `Product` through `OrderProduct` (composite key `OrderId+ProductId`, `Quantity`, `UnitPrice` precision 18,2).
- `CreateOrder` snapshots unit price from the product at order time and sums `TotalPrice`.

Provider: `Npgsql.EntityFrameworkCore.PostgreSQL`. No EF migrations in the repo yet.

## OpenAPI and Scalar

`AddApiVersioning().AddApiExplorer().AddOpenApi()` plus a second `AddOpenApi()`. Documents are mapped per version; Scalar lists each group.

Nested types in every slice are named `Request`, `Response`, `AddressRequest`, etc. The default OpenAPI schema id is the **bare type name**, so those collide and Scalar shows one body for every operation. `SchemaIds.Qualified` prefixes the declaring feature type (`CreateProductRequest`, `CreateCustomerAddressRequest`, …). Wired with `ConfigureAll<OpenApiOptions>` so versioned documents get it too.

## Local vs Docker

**Default development:** `docker compose up db`, then Rider **http** profile (`http://localhost:5039/scalar`). Development connection string targets `localhost` and must match the Compose Postgres user/db.

**Published stack:** `docker compose --profile app up --build` — API on `8080`. Compose interpolates `.env` into `ConnectionStrings__DefaultConnection` (host `db`). Profile `app` is opt-in so a plain `docker compose up` does not rebuild the API image.

`.env` is not baked into the image (`.dockerignore`). The app container does not receive `env_file`; only the connection-string environment variable.

## Checks

File-based apps under `Tools/` (compile-excluded from the web project). Run from **outside** the project directory with `--no-cache` or the runner reuses a stale referenced build:

```bash
dotnet run --no-cache /path/to/Tools/SchemaIdCheck.cs
dotnet run --no-cache /path/to/Tools/ResponseEntityCheck.cs
```

- `SchemaIdCheck` — nested request types must get distinct OpenAPI schema ids.
- `ResponseEntityCheck` — slice `Response` types must not reach `Domain.Entities` (guards the serialization cycle).

Local CLI builds on this machine may need `AllowMissingPrunePackageData=true` because the installed SDK is missing prune-package data. The Docker SDK image does not.

## Known holes

- `CancelOrder` is a no-op.
- Azure mail/key-vault classes are comments-only and not registered.
- `EnsureCreated` will not apply later model changes to an existing database.
- OpenAPI package graph currently resolves with NU1603 (Asp.Versioning.OpenApi rc vs stable ApiExplorer) and NU1903 (Microsoft.OpenApi advisory) — pre-existing restore warnings, not part of the slice design.
