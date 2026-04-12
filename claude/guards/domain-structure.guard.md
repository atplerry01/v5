# Domain Structure Guard

## Purpose

Enforce the canonical three-level nesting (`classification/context/domain`) across all layers, and the `-system` suffix rule that distinguishes the domain layer from all other layers.

## Scope

All directories under `src/`, `tests/`, and `infrastructure/`. Applies to every commit, PR, and CI pipeline run.

## Rules

### DS-R1: Domain layer MUST use `{classification}-system`

All top-level classification folders under `src/domain/` MUST use the `-system` suffix.

**Canonical form:** `src/domain/{classification}-system/{context}/{domain}/`

Example: `src/domain/operational-system/sandbox/todo/`

Exception: `src/domain/shared-kernel/` is not a classification and is exempt.

Violation: Any classification folder under `src/domain/` without the `-system` suffix (e.g., `src/domain/operational/`).

### DS-R2: Non-domain layers MUST NOT use `-system`

All layers outside `src/domain/` MUST use the raw classification name WITHOUT the `-system` suffix. This applies to:

- `src/engines/`
- `src/runtime/`
- `src/systems/`
- `src/platform/`
- `src/projections/`
- `src/shared/`
- `infrastructure/`
- `tests/`

**Canonical form:** `{classification}/{context}/{domain}/` (no `-system`)

Example: `src/projections/operational/sandbox/todo/`

Violation: Any folder or namespace segment containing `-system` or `System` (PascalCase equivalent) in a non-domain layer. The only exception is `using` directives that reference `Whycespace.Domain.{X}System.*` — these correctly point into the domain layer.

### DS-R3: All paths MUST follow `classification/context/domain`

Every domain concept must be reachable via exactly three levels of nesting below the layer root (or below an engine tier prefix like `T2E/`).

Violation: A domain placed directly under a classification without a context level (e.g., `src/projections/operational/todo/` — missing the `sandbox` context).

If the context cannot be inferred, use `default` as a temporary context.

### DS-R4: No flat domains allowed

A classification folder that contains domain folders directly (without a context intermediary) is a structural violation.

Violation: `src/projections/operational/todo/` instead of `src/projections/operational/sandbox/todo/`.

### DS-R5: Cross-layer mapping MUST be consistent

For every domain that exists in `src/domain/{classification}-system/{context}/{domain}/`, the corresponding paths in other layers must use the same `{context}/{domain}` segments with the raw classification (no `-system`):

- `src/engines/T2E/{classification}/{context}/{domain}/`
- `src/projections/{classification}/{context}/{domain}/`
- `src/systems/downstream/{classification}/{context}/{domain}/`
- `infrastructure/policy/domain/{classification}/{context}/{domain}/`
- `infrastructure/event-fabric/kafka/topics/{classification}/{context}/{domain}/`
- `infrastructure/data/postgres/projections/{classification}/`

Violation: Mismatched classification, context, or domain segments across layers.

### DS-R6: Namespace consistency with folder structure

C# namespaces MUST reflect the canonical folder path. Non-domain namespaces must use `Orchestration` (not `OrchestrationSystem`), `Constitutional` (not `ConstitutionalSystem`), etc.

Domain namespaces correctly use the PascalCase equivalent of the `-system` suffix: `Whycespace.Domain.OrchestrationSystem.*`.

### DS-R7: Classification strings in DomainRoute and CommandContext

`DomainRoute` and `CommandContext.Classification` MUST use the raw classification name without `-system`.

**Canonical:** `DomainRoute("operational", "sandbox", "todo")`

Violation: `DomainRoute("operational-system", "sandbox", "todo")` or `Classification = "orchestration-system"`.

## Severity

- DS-R1 through DS-R5: **S1** (architectural — structural drift)
- DS-R6, DS-R7: **S2** (structural — naming/reference inconsistency)
