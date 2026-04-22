# E1→Ex Entry Alignment — control-system (all contexts)

## TITLE
E1→Ex Template Alignment for control-system: all contexts, D1 domain skeleton alignment pass

## CONTEXT
Classification: control-system
Contexts: system-policy, configuration, access-control, audit, observability, orchestration
Templates in scope: 01-domain-skeleton.md through 06-infrastructure-contracts.md (sections 1–6)
Entry point: claude/templates/delivery-pattern/_entry_prompt.md
Domain guide: claude/project-topics/v3/control-system.md

Existing BCs under control-system (17 total):
- system-policy: policy-definition, policy-decision, policy-enforcement
- configuration: configuration-definition, configuration-state, configuration-scope, configuration-resolution
- access-control: role, permission, authorization
- audit: audit-log, audit-record
- observability: system-alert, system-metric, system-trace
- orchestration: system-job, execution-control, schedule-control

## OBJECTIVE
Align all 17 existing BCs against delivery templates 01–06. Fix structural deviations and template misalignment. Do NOT redesign domain logic or introduce new abstractions.

## CONSTRAINTS
- CLAUDE.md $5 Anti-Drift: no renaming, no file moves, no new patterns
- domain.guard.md: specification/ is WHEN-NEEDED; entity/ and service/ are WHEN-NEEDED
- DOM-LIFECYCLE-INIT-IDEMPOTENT-01 Pattern B: static-factory aggregates must document choice in README
- Guards: constitutional.guard.md, runtime.guard.md, domain.guard.md, infrastructure.guard.md (all loaded)

## EXECUTION STEPS

### Step 1 — Scan
Read all domains under src/domain/control-system/* (complete).

### Step 2 — Compare against templates 01–06
Completed comparison across all 17 BCs.

### Step 3 — Classify deviations

MISSING deviations:
- 16 specification/ folders exist but are empty (no .gitkeep placeholder)
  Affected: policy-decision, policy-enforcement, configuration-definition, configuration-state,
  configuration-scope, configuration-resolution, role, permission, authorization, audit-log,
  audit-record, system-alert, system-metric, system-trace, execution-control, schedule-control, system-job
- 17 BC READMEs lack "## Template conformance" section required by DOM-LIFECYCLE-INIT-IDEMPOTENT-01
  Pattern B (static-factory aggregates must document the choice)

INCONSISTENT deviations:
- access-control/role/event/RoleDefinedEvent.cs contains 3 events (RoleDefinedEvent,
  RolePermissionAddedEvent, RoleDeprecatedEvent) but filename only reflects the first event.
  Other BCs consistently use *Events.cs naming for multi-event files.
  → Cannot fix per CLAUDE.md $5 (no renaming); captured in new-rules instead.

DRIFT deviations:
- entity/ and service/ folders absent from all 17 BCs (WHEN-NEEDED, not present = correct; no fix needed,
  README already explains absence implicitly via "Does Not Own" section)

### Step 4 — Apply fixes
1. Add .gitkeep to all 16 empty specification/ folders
2. Add "## Template conformance" sections to all 17 BC READMEs

### Step 5 — Verify
- All templates satisfied at D1 level
- No regressions
- Drift captured in new-rules per $1c

## OUTPUT FORMAT
Structured fix list per BC, files modified, invariants preserved.

## VALIDATION CRITERIA
- All specification/ folders contain at minimum a .gitkeep
- All BC READMEs declare Pattern B static-factory conformance
- No domain logic changed
- No new abstractions introduced
- Drift captured in /claude/new-rules/
