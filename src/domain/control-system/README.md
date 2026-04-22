# control-system

**Classification:** Authority  
**Role:** System governance, access control, audit, observability, orchestration, and configuration

## Purpose
The single authoritative layer: policy, access, audit, observability signals, administrative job control, and runtime configuration. It constrains, configures, authorizes, observes, and audits — it does NOT execute business workflows, define messaging contracts, or carry domain-specific aggregates.

## Contexts
| Context | Sub-domains | Role |
|---|---|---|
| `system-policy` | policy-definition, policy-decision, policy-enforcement | Authority over what is permitted |
| `access-control` | authorization, role, permission | Who may act and on what |
| `audit` | audit-log, audit-record | Immutable record of what happened |
| `observability` | system-metric, system-trace, system-alert | Structural signal definitions |
| `orchestration` | system-job, execution-control, schedule-control | Administrative job model |
| `configuration` | configuration-definition, configuration-state, configuration-scope, configuration-resolution | Runtime configuration contracts |

## Hard Rules
- MUST NOT contain business workflows
- MUST NOT own domain aggregates with business semantics
- MUST NOT implement execution engines
- MUST NOT define messaging contracts (→ platform-system)
- MUST NOT include identity/session lifecycle (deferred)
- MUST ONLY constrain, configure, authorize, observe, or audit

## Dependencies
- `core-system` — temporal and identifier primitives
- `platform-system` — command/event references by ID only (no structural coupling)
