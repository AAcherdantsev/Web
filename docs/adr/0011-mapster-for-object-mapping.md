# 0011. Use Mapster for object mapping

Date: 2026-10-16
Status: Accepted

## Context

Endpoints map domain entities to response DTOs. Some popular mapping libraries require a commercial licence for production use. The project is meant to be deployable without a paid licence, so the mapping library must be free for production use.

## Decision

We use **Mapster 10** (MIT licence). Mappings are declared in `MappingConfig`, which implements Mapster's `IRegister` and is discovered at startup with `TypeAdapterConfig.GlobalSettings.Scan`. Endpoints call `Adapt<T>()`, so they do not inject a mapper.

Most mappings are by convention (matching property names). Only the paged list needs an explicit mapping.

## Consequences

- No licence key is needed.
- Endpoints have fewer constructor parameters, and tests do not need to mock the mapper. `MappingConfigTests` checks the mapping directly.
- Mapster uses reflection at runtime. For this data volume the cost is negligible. If mapping ever becomes a measured bottleneck, Riok.Mapperly generates the mapping code at compile time and is also MIT-licensed.
- The `Adapt` extension is a global entry point, so mapping configuration is global. This is acceptable for one application.

## Alternatives considered

- **Commercially licensed mapping libraries.** Mature, but they need a paid licence for production use, which would be a deployment obligation for the project.
- **Manual mapping.** Explicit and dependency-free. For a few mappings it would also be a good choice. We kept a library to avoid writing and maintaining mapping code for every new DTO.
- **Riok.Mapperly.** Compile-time generation with no runtime reflection. A strong option that we may adopt later.
