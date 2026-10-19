# 0017. Operational endpoints and telemetry

Date: 2026-10-16
Status: Accepted

## Context

An orchestrator (for example Kubernetes or a container platform) needs to know whether an instance is alive and whether it can serve traffic. Operators need traces and metrics to find slow requests and failing dependencies.

## Decision

* **Health checks** are split in two:
  * `GET /health/live` reports that the process responds. It checks nothing else, so a database outage does not restart the instance.
  * `GET /health/ready` also checks the database (`CanConnectAsync`). In the `Mock` data source there is no database to check, so the endpoint reports healthy. The readiness probe is intended to take an instance out of rotation when its database is unreachable.
* **Telemetry** uses OpenTelemetry:
  * traces for ASP.NET Core requests and SQL Server calls;
  * metrics for ASP.NET Core and the .NET runtime;
  * export over OTLP to the endpoint in `OTEL_EXPORTER_OTLP_ENDPOINT`.
  The service name is `web-api`. Logs stay on the console, and they are not exported through OpenTelemetry.
* The health endpoints are not rate limited, and they are not in the Swagger document.

## Consequences

* Probes can be configured without custom code in the platform.
* Traces show where a slow request spends its time, including SQL calls.
* The exporter needs a reachable OTLP endpoint. Without one, export fails and the failures are logged as warnings. The service keeps running.
* The readiness check opens a database connection on each probe. Probes should not run more often than every few seconds.
* The OpenTelemetry packages are licensed under Apache-2.0 and are listed in `THIRD-PARTY-NOTICES.md`.

## Alternatives considered

* **A single health endpoint.** Cannot tell "restart me" from "do not send me traffic", so one database outage would restart every instance.
* **Application Insights or another vendor SDK.** Ties the service to one vendor. OpenTelemetry works with most backends, and the Aspire dashboard reads the same data.
* **Logs through OpenTelemetry.** Not needed while the platform collects console output.
