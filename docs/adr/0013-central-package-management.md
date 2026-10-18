# 0013. Central package management

Date: 2026-10-16
Status: Accepted

## Context

The solution has seven projects. Without central management, a package version is repeated in each project file, and the versions drift apart. Transitive dependencies with known vulnerabilities (for example MessagePack and System.Security.Cryptography.Pkcs) have to be updated in every project that uses them.

## Decision

Package versions are defined only in `Directory.Packages.props` (`ManagePackageVersionsCentrally`). Project files reference packages without a version. Transitive packages are pinned in the same file (`CentralPackageTransitivePinningEnabled`). `Directory.Build.props` holds settings shared by all projects.

## Consequences

- A version is changed in one place, and every project uses the same version.
- Vulnerabilities in transitive dependencies can be fixed by pinning them centrally. `dotnet list package --vulnerable --include-transitive` should be run before a release.
- The file must be edited for every package change. The README describes the steps.

## Alternatives considered

- **Versions in each project file.** Common, but versions drift and security fixes are easy to miss.
- **Version variables in a `.props` file.** Works, but it is the older mechanism that Central Package Management replaces.
