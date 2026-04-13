---
name: contracts-boundary
type: structural
severity: S1
locked: true
---

# Contracts Boundary Enforcement Guard

## Purpose
Preserve strict separation between business language (domain contracts) and
system mechanics (infrastructure/runtime contracts) within `src/shared/contracts/`.

## Rules

### G-CONTRACTS-01 — Domain Contracts Location
FAIL IF any domain-specific contract (command, query, intent, DTO, read model,
policy ID) exists outside the canonical path:
`src/shared/contracts/{classification}/{context}/{domain}/`

Domain classifications: `operational`, `economic`, `governance`, and future
classifications as they are onboarded.

### G-CONTRACTS-02 — System Contracts Location
FAIL IF any system-level contract exists inside a domain classification path.
System contracts MUST live under one of:
- `contracts/events/{classification}/{context}/{domain}/` (event schemas only)
- `contracts/infrastructure/` (persistence, messaging, health, policy, projection, chain, admission)
- `contracts/runtime/` (command dispatch, workflow execution, control plane)
- `contracts/common/` (API envelope, standard response models)
- `contracts/engine/` (generic engine contracts)
- `contracts/event-fabric/` (event infrastructure)
- `contracts/identity/` (identity/auth)
- `contracts/chain/` (chain integrity/sequencing)
- `contracts/policy/` (system-wide policy evaluation contracts)
- `contracts/projection/` (projection infrastructure)
- `contracts/projections/{classification}/{context}/{domain}/` (cross-domain projection read models)

### G-CONTRACTS-03 — Prohibited Top-Level Directories
FAIL IF any of the following directories are introduced under `src/shared/contracts/`:
- `contracts/domain/`
- `contracts/business/`
- `contracts/core/`

These generic groupings violate the classification-based domain structure.

### G-CONTRACTS-04 — No Domain/System Mixing
FAIL IF a single directory contains both domain-specific contracts and
system-level contracts. Domain and system concerns occupy separate directory
subtrees with no overlap.

### G-CONTRACTS-05 — Event Schema Location
FAIL IF event schemas exist outside `contracts/events/{classification}/{context}/{domain}/`.
Event schemas follow the domain three-level nesting but live under the `events/`
subtree, not alongside commands/queries in the domain contract path.

### G-CONTRACTS-06 — Namespace Alignment
FAIL IF a contract file's declared namespace does not align with its directory
path. Domain contracts use `Whyce.Shared.Contracts.{Classification}.{Context}.{Domain}`.
System contracts use `Whyce.Shared.Contracts.{Category}` (e.g., `Infrastructure.Health`,
`Runtime`, `Common`). Event schemas use
`Whyce.Shared.Contracts.Events.{Classification}.{Context}.{Domain}`.

## Severity
- All G-CONTRACTS-* rules: **S1 — HIGH** (block merge)
