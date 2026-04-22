# control-system / observability / system-trace

**Classification:** control-system  
**Context:** observability  
**Domain:** system-trace

## Purpose
Structural definition of a distributed trace span: operation name, parent span linkage, timing boundaries, and status. Defines the canonical contract for how spans are recorded and correlated across the control plane.

## Scope
- Span definition: operation name, parent span ID (optional), start/end timestamps, status (success|error|timeout)
- Baggage propagation schema: declared key names only — no values in domain

## Does Not Own
- Span export or sampling (→ infrastructure)
- Trace storage (→ runtime)

## Inputs
- Operation name, parent span ID (optional), start timestamp

## Outputs
- `SystemTraceSpanStartedEvent`
- `SystemTraceSpanCompletedEvent`

## Invariants
- Span ID is deterministic: SHA256 of (traceId + operationName + startedAt)
- Root spans have no parent span ID
- End timestamp is always after start timestamp on completion

## Dependencies
- `core-system/identifier` — span and trace IDs
- `core-system/temporal` — span timing

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `SystemTraceAggregate.Start(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
