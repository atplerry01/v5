# control-system / orchestration

**Classification:** control-system  
**Context:** orchestration

## Purpose
Administrative orchestration model: structural definitions for how system-level jobs are controlled, scheduled, and executed. This context owns the control model — not execution logic. No business workflows, no sagas, no compensation chains.

## Sub-domains
| Domain | Responsibility |
|---|---|
| `execution-control` | Control record for a system job instance: start, stop, suspend signals |
| `system-job` | Definition of a system-level administrative job |
| `schedule-control` | Scheduling contract for recurring or deferred job execution |

## Removed (violations)
- `workflow-definition` — workflow semantics; belongs in engine/runtime layer
- `checkpoint` — execution state; belongs in engine layer
- `compensation` — saga compensation; belongs in engine layer

## Does Not Own
- Business workflow execution (deferred — domain semantics)
- Policy evaluation during job execution (→ system-policy)
- Job scheduling implementation (→ runtime)

## Invariants
- A job instance references exactly one system-job definition
- An execution-control record references exactly one job instance
- Schedule controls reference valid system-job definitions

## Dependencies
- `core-system/identifier` — job, instance, and control IDs
- `core-system/temporal` — scheduling windows and execution timing
