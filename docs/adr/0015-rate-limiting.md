# 0015. Rate limiting with token buckets per client

Date: 2026-10-16
Status: Accepted

## Context

The API is public, and a single client can send requests faster than the database and the service can handle. Without a limit, one client, a bug in a client that retries in a loop, or a scraper can degrade the service for everyone. Every product list request reads from the database, so read traffic is the main cost.

Rate limiting is not part of the task specification, but it is needed before the API runs in a shared or public environment.

## Decision

We use the **built-in ASP.NET Core rate limiter** (`Microsoft.AspNetCore.RateLimiting`, part of the shared framework, so there is no extra dependency). Limits are enforced per client with the **token bucket** algorithm, in two policies:

| Policy | Applies to | Burst (`TokenLimit`) | Refill (`TokensPerPeriod` per `ReplenishmentPeriodSeconds`) | Default meaning |
| --- | --- | --- | --- | --- |
| `reads` | `GET` endpoints | 100 | 100 per 60 s | up to 100 reads at once, then about one every 0.6 s |
| `writes` | `POST` and `PATCH` endpoints | 20 | 20 per 60 s | up to 20 writes at once, then about one every 3 s |

Details of the decision:

- **Clients are identified by IP address.** Each policy keeps its own bucket for each client, so heavy reading does not use up the writing allowance.
- **Requests are not queued.** A request over the limit is rejected immediately with `429 Too Many Requests`, a `Retry-After` header, and a problem details body (`application/problem+json`, see [0009](0009-problem-details-for-errors.md)). Queueing would hold connections open and add latency, which is worse for clients than a clear rejection.
- **Limits are configuration.** They are bound from the `RateLimiting` section and validated at startup, so an invalid value (for example a bucket of size 0) stops the application instead of silently disabling the limit (see [0010](0010-pagination.md) for the same approach).
- **Policies are attached per endpoint** with FastEndpoints (`Options(b => b.RequireRateLimiting(...))`), so each endpoint declares its policy in its own class.

## Why a token bucket

Four algorithms were considered. Each one is described below, with the reason it was or was not chosen.

- **Fixed window** (for example 100 requests per calendar minute). The simplest to build and explain, and its state is one counter per window. Its weakness is the window boundary: a client can send 100 requests at second 59 and 100 more at second 61, which is 200 requests within two seconds while the limit is 100 per minute.
- **Sliding window** (log of timestamps, or segmented counters). Removes the boundary problem and enforces the limit smoothly. The log needs memory proportional to the number of requests in the window, and segmented counters are an approximation. Accurate sliding windows cost more than this service needs.
- **Leaky bucket.** Turns bursts into a steady output rate by queueing requests. It is a good fit for traffic shaping, but it needs a queue and adds latency, and it does not give clients a clear answer when they are over the limit. We do not queue (see above).
- **Concurrency limiter.** Limits the number of requests being processed at the same time. It protects the database from slow queries, but it does not limit the request rate, so a fast client can still send unlimited requests. It is a useful complement later, not a replacement.

**Token bucket** was chosen because:

1. It allows **short bursts** up to the bucket size and enforces the **long-run average**. Real clients behave this way: a page can load several products at once, then stay idle.
2. Its state is small: a token count and a timestamp per client.
3. Its parameters are easy to explain to API consumers: the burst size and the sustained rate.
4. The built-in implementation supports rejection without queueing, which matches the behaviour we want.

## Consequences

Accepted trade-offs and limits of this design:

- **Per-instance state.** Buckets are kept in memory. With N instances behind a load balancer, a client can make about N times the limit. Scaling out needs a shared store (for example Redis) or limits enforced at the edge.
- **Shared IP addresses.** Clients behind the same NAT or corporate proxy share one bucket. Behind a reverse proxy, every client appears to come from the proxy's address, so the limit becomes global. This is fixed by configuring trusted forwarded headers, which the project does not do yet. Keying by authenticated user will be better once authentication exists.
- **Memory grows with the number of distinct clients.** Each new client IP adds a bucket. Idle buckets should be evicted; this should be verified under load before production.
- **`Retry-After` is conservative.** The built-in token bucket reports the refill period (60 seconds) rather than the time until the next token (about 0.6 seconds for reads). Clients wait longer than needed, which is safe.
- **Clients must handle `429`.** Documented in the Swagger responses of each endpoint.
- **Swagger and future health checks are not limited.** They are not in a policy, which is intended.

## Alternatives considered

- **Reverse proxy or API gateway** (nginx, YARP, Azure Front Door, API Management). Better for production at the edge, because it protects the service before requests reach the application and it can share state across instances. It is complementary: the application keeps its own limits as a second layer, in case the proxy is bypassed or misconfigured.
- **AspNetCoreRateLimit** (MIT). A widely used package, but its last major maintenance was before the built-in limiter existed, and it adds a dependency for what the framework now provides.
- **Custom middleware with a dictionary of counters.** Easy to get wrong (thread safety, eviction, memory) and gives no advantage over the built-in limiter.
