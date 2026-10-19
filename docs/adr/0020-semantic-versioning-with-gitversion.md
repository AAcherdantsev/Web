# 0020. Semantic versions calculated from git history with GitVersion

Date: 2026-10-19
Status: Accepted

## Context

The API needs version numbers for releases, and a release must be traceable to a commit. A version typed by hand in a project file drifts from the history, and it is easy to forget to change it. The repository is developed on feature branches and merged into `main`, so the history already shows what changed since the last release.

## Decision

We calculate the semantic version with **GitVersion 6** from the git history. It is a local tool (`.config/dotnet-tools.json`), and its settings are in `GitVersion.yml`:

- The mode is `ContinuousDelivery`, so every commit has a version, and the version is not reset by the pull request branch.
- The first release is `1.0.0`. Until a release tag exists, GitVersion counts the commits since that version.
- Commits on `main` are patch increments. Feature branches get the `alpha` pre-release label.

The pull request check (`.github/workflows/pr-check.yml`) calculates the version and writes it to the job summary. The check uses the full history (`fetch-depth: 0`), because GitVersion needs it. A maintainer creates the release tag (`vX.Y.Z`) on `main` when a release is made. The version is not written into the assemblies, because that would need a build-time package (see the alternatives).

## Consequences

- The version comes from the repository, so it cannot be forgotten or set by hand.
- Every pull request shows the version it would produce, which makes version bumps visible during review.
- The full history is required. A shallow clone calculates a wrong version, so the check must not use the default `fetch-depth`.
- The tag, not the code, is the release decision. Creating `vX.Y.Z` is a manual step.
- The local tool is pinned in the tool manifest, so every developer and the CI calculate the same version.

## Alternatives considered

- **A version property in `Directory.Build.props`.** Simple, but it has to be edited for each release, which is the drift this decision avoids.
- **GitVersion.MsBuild or MinVer (build-time packages).** They put the version into the assemblies, but their package is then a top-level reference of every project. It would appear in `THIRD-PARTY-NOTICES.md` as a package that ships with the API, which it does not. We can add it later if the assemblies need the version.
- **Nerdbank.GitVersioning.** Similar to GitVersion, with the same build-time package drawback. We picked GitVersion because the team can run it as a command line tool.
