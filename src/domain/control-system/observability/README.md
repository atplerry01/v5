# control-system / observability

**Classification:** control-system  
**Context:** observability

## Purpose
Owns the domain model for control-plane observability signals. These are structural contracts — not implementations. The platform layer emits them; this context defines their canonical shape.

## Sub-domains
| Domain | Responsibility |
|---|---|
| `system-metric` | Metric definition: name, type, unit, label schema |
| `system-trace` | Trace span definition: operation, parent linkage, timing |
| `system-alert` | Alert definition: condition expression, severity, notification target |

## Does Not Own
- Metric collection or aggregation (→ runtime / infrastructure)
- Alert delivery or notification (→ infrastructure)
- Business KPIs or analytics (deferred — domain semantics)
- `system-health` or `system-signal` (removed — too broad; alert covers the constraint case)

## Invariants
- Metric names follow `{classification}_{context}_{name}` convention
- Trace spans reference a valid parent span or are declared as root spans
- Alerts reference a declared metric definition

## Dependencies
- `core-system/identifier` — signal IDs
- `core-system/temporal` — span timing and alert effective windows
