# 0018. Migrations as a separate step in Production

Date: 2026-10-16
Status: Accepted

## Context

The API applies its database migrations on startup. With several instances starting together, each one tries to migrate. A failed migration stops an instance after it has already started to take traffic, and there is no point at which the migration can be checked before the new version goes live.

## Decision

**Migrations.** Migrations run as a separate step with `--migrate-only`. The command applies the pending migrations and exits with code `0`, or with a non-zero code on failure. In Production, `Database:MigrateOnStartup` is `false`, so starting the API does not change the schema. In development it is `true`, so `dotnet run` works without extra steps.

**Startup validation.** The application refuses to start, with a message that names the setting, when `Mock` is used in Production or when the `WebDb` connection string is missing.

## Consequences

* A release can be checked: the migration step fails before the new version starts.
* Several instances can start at the same time without racing on the schema.
* Rollback of a migration is not automatic. It needs a backup taken before the migration (see `docs/operations.md`).
* The migration step needs the same connection string as the API, so it must run where the database is reachable.
* In the development start, the first migration is retried every two seconds for up to a minute. A SQL Server container accepts connections a few seconds before it is ready, and without the retry the API stopped at startup. This applies to the `--migrate-only` step too.

## Alternatives considered

* **Keep migrating on startup.** Simple for one instance, unsafe for several.
* **Migration bundles** (`dotnet ef migrations bundle`). A good option for larger teams. The `--migrate-only` mode uses the same migrations and needs no extra tool, so it was chosen for now.
