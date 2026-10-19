# 0001. Record architecture decisions

Date: 2026-10-16
Status: Accepted

## Context

Decisions about this project are made over time, and they affect several parts of the solution. Without a written record, the reasons behind a choice are lost, and the same questions come up again. For example, the reasons for using FastEndpoints and FluentResults should be found in the repository, not in someone's memory.

## Decision

We record significant architecture decisions as numbered Architecture Decision Records in `docs/adr/`, using the template in `docs/adr/README.md`. A decision is significant when it affects the structure, a dependency, a public contract, or the way the system is operated.

## Consequences

- Decisions and their trade-offs are visible to new contributors and reviewers.
- Writing a record takes a little time for each significant change.
- Records are append-only: a changed decision gets a new record that supersedes the old one.

## Alternatives considered

- **Wiki or chat history.** Not versioned with the code, and hard to find later.
- **Comments in the code.** Explain how the code works, not why it was chosen. They are not a good place for decisions that affect several components.
