# 0007. Use .NET Aspire for local orchestration

Date: 2026-10-16
Status: Accepted

## Context

The task asks for a project that can be run in an IDE. The database mode needs SQL Server running in Docker with a password and a persistent data folder. Starting these by hand is error-prone.

## Decision

The `Web.AppHost` project uses **.NET Aspire 13** (Aspire.Hosting.AppHost, SDK version 13.6.1). It:

- starts SQL Server in a container only when `DataSource` is `Database` (the default),
- passes the data source setting to the API,
- waits for the database before starting the API,
- provides the Aspire dashboard with logs, traces, and the API's address.

The SQL password is a parameter stored in user secrets, so it is not committed.

## Consequences

- One command starts the complete local environment.
- The AppHost requires Docker in database mode and is specific to .NET. The API itself runs without Aspire.
- The AppHost's own dependencies are development tools. They are not part of the API output, so the notices generator, which reads the API project, does not list them.
- Aspire 13 uses the SDK, not a workload, so setup scripts do not install a workload.

## Alternatives considered

- **Docker Compose.** Works well with containers, but it does not give the .NET developer a single entry point, the dashboard, or service discovery.
- **Manual setup.** The README would be long, and every developer would configure the environment differently.
