# E1 → EX Domain Conformance Audit (Canonical)

**Validates:** [`claude/templates/delivery-pattern/`](../templates/delivery-pattern/) — specifically [01-domain-skeleton.md](../templates/delivery-pattern/01-domain-skeleton.md) and [05-quality-gates.md](../templates/delivery-pattern/05-quality-gates.md) Gate 1.
**Type:** Pure validation layer — defines NO rules. All rules live in the template and in `domain.guard.md`.
**Scope:** `src/domain/{classification}-system/**` for every classification declared in [00-section-checklist.md](../templates/delivery-pattern/00-section-checklist.md).
**Run cadence:** On every prompt execution per CLAUDE.md $1b.

---

## Classification Execution Tiers (LOCKED 2026-04-22)

Not all classifications require the full E1→EX stage set. Classifications are partitioned as:

| Tier | Label | E1→EX scope | Classifications |
|---|---|---|---|
| **Tier 1** | Non-executable system | E1 + E7 (serialization) + E12 (tests). E2/E4/E5/E6/E8/E9/E10/E11 = N/A by design. | `core-system` |
| **Tier 2** | Executable system | Full E1→EX (all stages where applicable per context) | All other classifications |

**Tier 1 rationale:** `core-system` owns only immutable value-object primitives (temporal, ordering, identifier). It has no commands, events, engines, policies, projections, or APIs by design (topic file topics 6, 10, 12). E1→EX completion means: structural compliance + serialization integrity + determinism guarantees + test coverage. User confirmed 2026-04-22: `core-system` = **"Non-executable system (E1-only with extended validation)"**. Rule captured in `claude/new-rules/20260422-120000-core-system-classification.md`.

---

## Purpose

Verify that each bounded context in every declared vertical classification (`economic-system`, `structural-system`, `content-system`, `trust-system`, `constitutional-system`, `business-system`, `core-system`, `decision-system`, `intelligence-system`, `operational-system`, `orchestration-system`) conforms to the **E1 → EX Delivery Pattern v1.0** at the domain layer (sections 1–6), respecting the Tier 1/Tier 2 stage restrictions above.

This audit complements but does not replace [`domain.audit.md`](domain.audit.md):

- `domain.audit.md` validates WBSM v3 canonical domain rules (purity, naming, nesting, DTO, behavioral) against any code that exists.
- **This audit** validates D-level promotion readiness per E1 → EX — specifically the event-sourcing contract (AggregateRoot inheritance, Version, LoadFromHistory), the lifecycle-init guard, and JSON replay losslessness.

A BC can pass `domain.audit.md` and still fail this audit if it implements the domain layer but uses a non-canonical event-sourcing pattern (for example the manual `_uncommittedEvents` list pattern found across `structural-system` during the 2026-04-19 gap audit).

## Source template

This audit checks the rules declared in:

- [01-domain-skeleton.md](../templates/delivery-pattern/01-domain-skeleton.md) — folder layout, naming, aggregate pattern, VO pattern, event pattern, error pattern, specification pattern.
- [05-quality-gates.md](../templates/delivery-pattern/05-quality-gates.md) Gate 1 — 10 auditable checks.

It cross-references but does not re-declare rules from:

- [`domain.guard.md`](../guards/domain.guard.md) — D-VO-TYPING-01, D-ERR-TYPING-01, DOM-LIFECYCLE-INIT-IDEMPOTENT-01, D-PURITY-01, D-DET-01, core purity rules 1–24, DS-R1..R8.
- [`constitutional.guard.md`](../guards/constitutional.guard.md) — INV-REPLAY-LOSSLESS-VALUEOBJECT-01, GE-01 (determinism), HSID G5 (domain purity from HSID engine).
- [`runtime.guard.md`](../guards/runtime.guard.md) — no-dead-code, stub-detection (zero `NotImplementedException`, zero `// TODO`).

---

## Rule index

| Rule | Severity | Scope | Source |
|---|---|---|---|
| E1XD-FOLDER-MUST-01 — MUST subfolders present: `aggregate/`, `error/`, `event/`, `value-object/` | S1 | per BC | 01-domain-skeleton.md §Artifact subfolders |
| E1XD-FOLDER-OPT-01 — WHEN-NEEDED subfolders (`entity/`, `service/`, `specification/`) present OR justified-as-omitted in BC README | S2 | per BC | 01-domain-skeleton.md §Artifact subfolders |
| E1XD-NAMING-AGG-01 — aggregate named `{Concept}Aggregate` | S2 | per aggregate file | 01-domain-skeleton.md §Naming rules |
| E1XD-NAMING-EVT-01 — event named `{Subject}{PastVerb}Event` | S2 | per event file | 01-domain-skeleton.md §Naming rules |
| E1XD-AGG-INHERIT-01 — aggregate inherits `AggregateRoot` | **S0** | per aggregate | 01-domain-skeleton.md §Aggregate pattern |
| E1XD-AGG-VERSION-01 — aggregate exposes `Version` via base | **S0** | per aggregate | 01-domain-skeleton.md §Aggregate pattern |
| E1XD-AGG-LOADHIST-01 — aggregate supports `LoadFromHistory` via base | **S0** | per aggregate | 01-domain-skeleton.md §Aggregate pattern |
| E1XD-AGG-RAISE-01 — no manual `_uncommittedEvents` list | S1 | per aggregate | 01-domain-skeleton.md §Aggregate pattern |
| E1XD-AGG-LIFECYCLE-01 — lifecycle-init guard `Version >= 0` | S1 | per aggregate | DOM-LIFECYCLE-INIT-IDEMPOTENT-01 |
| E1XD-AGG-EVENT-REQUIRED-01 — every state change raises an event | S1 | per aggregate method | 01-domain-skeleton.md §Aggregate pattern |
| E1XD-EVENT-JPN-01 — `[JsonPropertyName("AggregateId")]` on AggregateId parameter | S1 | per event carrying AggregateId | INV-REPLAY-LOSSLESS-VALUEOBJECT-01 |
| E1XD-EVENT-PAST-01 — events are past-tense | S2 | per event file | domain.guard.md rule 6 |
| E1XD-VO-IMMUTABLE-01 — VOs are `readonly record struct` with `{ get; }` | S1 | per VO file | 01-domain-skeleton.md §Value Object pattern |
| E1XD-VO-VALIDATE-01 — VO constructors validate via `Guard.Against(...)` | S1 | per VO file | 01-domain-skeleton.md §Value Object pattern |
| E1XD-VO-PROPNAME-01 — single-primitive VO uses `Value` or `Code` | S2 | per single-primitive VO | 01-domain-skeleton.md §Value Object pattern + converter factory |
| E1XD-VO-TYPING-01 — identifiers use VO types, not raw `Guid`/`string` | S1 (string) / S2 (Guid) | per aggregate/event/service/spec | D-VO-TYPING-01 |
| E1XD-ERR-NOBCL-01 — no raw BCL exception throws | S1 | per domain file | D-ERR-TYPING-01 |
| E1XD-ERR-FACTORY-01 — errors live in `{Concept}Errors.cs` static factories | S2 | per BC | 01-domain-skeleton.md §Error pattern |
| E1XD-SPEC-PURE-01 — specifications are pure (no I/O, no DateTime, no DI) | S1 | per spec file | 01-domain-skeleton.md §Specification pattern |
| E1XD-SVC-STATELESS-01 — services are stateless | S1 | per service file | domain.guard.md rule 8 |
| E1XD-DET-NORNG-01 — no `Guid.NewGuid`, `DateTime.*`, `Random` | **S0** | per domain file | GE-01 / B-ID-01 |
| E1XD-DI-NONE-01 — no `Microsoft.Extensions.DependencyInjection` imports | S1 | per domain file | D-PURITY-01 |
| E1XD-STUB-NONE-01 — no `NotImplementedException`, no `// TODO`, no empty methods | S1 | per domain file | runtime.guard.md no-dead-code |
| E1XD-NAMESPACE-01 — namespace matches `Whycespace.Domain.{System}.{Context}.{Domain}` | S2 | per domain file | domain.guard.md rule 20 |

---

## Check procedures

### Folder + naming (E1XD-FOLDER-MUST-01, E1XD-FOLDER-OPT-01, E1XD-NAMING-AGG-01, E1XD-NAMING-EVT-01, E1XD-NAMESPACE-01)

For each `src/domain/{cls}-system/{ctx}/{dom}/`:

```bash
# MUST subfolders (always required)
for sub in aggregate error event value-object; do
  [ -d "src/domain/{cls}-system/{ctx}/{dom}/$sub" ] || echo "FAIL E1XD-FOLDER-MUST-01 missing $sub in {dom}"
done

# WHEN-NEEDED subfolders: if absent, require a justification line in the BC README
for sub in entity service specification; do
  if [ ! -d "src/domain/{cls}-system/{ctx}/{dom}/$sub" ]; then
    grep -q "no \`$sub/\`" "src/domain/{cls}-system/{ctx}/{dom}/README.md" 2>/dev/null \
      || echo "FAIL E1XD-FOLDER-OPT-01 {dom} is missing $sub/ and has no justification in README"
  fi
done

# Aggregate naming
find src/domain/{cls}-system/{ctx}/{dom}/aggregate -name '*.cs' | grep -v 'Aggregate\.cs$' \
  && echo "FAIL E1XD-NAMING-AGG-01"

# Event naming
find src/domain/{cls}-system/{ctx}/{dom}/event -name '*.cs' | grep -v 'Event\.cs$' \
  && echo "FAIL E1XD-NAMING-EVT-01"

# Namespace
grep -rL '^namespace Whycespace\.Domain\.' src/domain/{cls}-system/ \
  && echo "FAIL E1XD-NAMESPACE-01"
```

### Aggregate conformance (E1XD-AGG-INHERIT-01, E1XD-AGG-VERSION-01, E1XD-AGG-LOADHIST-01, E1XD-AGG-RAISE-01, E1XD-AGG-LIFECYCLE-01, E1XD-AGG-EVENT-REQUIRED-01)

For every `*Aggregate.cs`:

```bash
# Must inherit AggregateRoot (S0)
grep -L ': AggregateRoot' src/domain/{cls}-system/**/aggregate/*Aggregate.cs \
  && echo "FAIL E1XD-AGG-INHERIT-01"

# Must NOT manage _uncommittedEvents manually
grep -l '_uncommittedEvents' src/domain/{cls}-system/**/aggregate/*Aggregate.cs \
  && echo "FAIL E1XD-AGG-RAISE-01"

# Must have lifecycle-init guard on first-event methods
# (manual review — every Open/Create/Initialize/Define method checks Version >= 0)

# Must call RaiseDomainEvent from base, not manual append
grep -rE 'RaiseDomainEvent\(' src/domain/{cls}-system/**/aggregate/*Aggregate.cs \
  || echo "FAIL E1XD-AGG-EVENT-REQUIRED-01 if any method mutates state without raising"
```

Version and LoadFromHistory checks are inherited from `AggregateRoot` — E1XD-AGG-VERSION-01 and E1XD-AGG-LOADHIST-01 pass automatically when E1XD-AGG-INHERIT-01 passes, but fail if the aggregate declares its own `Version` field (shadowing the base).

### Event conformance (E1XD-EVENT-JPN-01, E1XD-EVENT-PAST-01)

For every `*Event.cs` under `src/domain/{cls}-system/**/event/`:

```bash
# Past-tense verb (heuristic: filename ends with common past-tense suffixes or known verbs)
# Manual review — grep for common non-past-tense patterns
grep -rEn 'Event\(|record \w+Event' src/domain/{cls}-system/**/event/ \
  | grep -Ev '(Created|Opened|Closed|Updated|Activated|Deactivated|Suspended|Archived|Restored|Approved|Rejected|Completed|Failed|Cancelled|Initiated|Confirmed|Published|Released|Revoked|Assigned|Unassigned|Escalated|Locked|Unlocked|Funded|Credited|Debited|Transferred|Allocated|Released|Reserved|Settled|Posted|Charged|Defined|Established|Registered|Submitted|Rated|Scored)' \
  && echo "REVIEW E1XD-EVENT-PAST-01 — manual verify past-tense"

# [JsonPropertyName("AggregateId")] on aggregate id parameter
# For every event whose first parameter is an aggregate identifier VO:
grep -rEn '\w+Id \w+Id\b' src/domain/{cls}-system/**/event/ \
  | while read line; do
      grep -l 'JsonPropertyName("AggregateId")' "$(echo "$line" | cut -d: -f1)" \
        || echo "FAIL E1XD-EVENT-JPN-01 in $line"
    done
```

### VO conformance (E1XD-VO-IMMUTABLE-01, E1XD-VO-VALIDATE-01, E1XD-VO-PROPNAME-01, E1XD-VO-TYPING-01)

```bash
# record struct immutability — no mutable setters
grep -rE '{ get; set; }' src/domain/{cls}-system/**/value-object/ \
  && echo "FAIL E1XD-VO-IMMUTABLE-01"

# Validation via Guard.Against — every explicit constructor with >0 parameters should call Guard.Against
grep -rB1 'public \w+\(\w' src/domain/{cls}-system/**/value-object/*.cs \
  | grep -v 'Guard\.Against' \
  && echo "REVIEW E1XD-VO-VALIDATE-01 — manual verify constructors validate"

# Strongly-typed identifier discipline
grep -rEn 'public Guid \w+Id|Guid \w+Id[,)]|string \w+Id[,)]' src/domain/{cls}-system/ \
  && echo "FAIL E1XD-VO-TYPING-01"
```

### Error conformance (E1XD-ERR-NOBCL-01, E1XD-ERR-FACTORY-01)

```bash
# No raw BCL exception throws
grep -rEn 'throw new (ArgumentException|ArgumentNullException|ArgumentOutOfRangeException|InvalidOperationException|NotSupportedException|NotImplementedException|Exception)\b' \
  src/domain/{cls}-system/ \
  && echo "FAIL E1XD-ERR-NOBCL-01"

# Errors factored into {Concept}Errors.cs
for bc in src/domain/{cls}-system/*/*/; do
  ls "${bc}error/"*Errors.cs 2>/dev/null \
    || echo "FAIL E1XD-ERR-FACTORY-01 no Errors.cs in $bc"
done
```

### Specification + service purity (E1XD-SPEC-PURE-01, E1XD-SVC-STATELESS-01)

```bash
# Specifications: no async/await, no DateTime, no DbContext, no HttpClient
grep -rEn 'async|await|DateTime\.|IClock|DbContext|HttpClient|IHttpClientFactory' \
  src/domain/{cls}-system/**/specification/ \
  && echo "FAIL E1XD-SPEC-PURE-01"

# Services: no mutable instance fields
grep -rEn 'private \w+ _\w+\s*[;=]' src/domain/{cls}-system/**/service/ \
  | grep -v 'readonly' \
  && echo "REVIEW E1XD-SVC-STATELESS-01 — manual verify no mutable state"
```

### Global forbidden patterns (E1XD-DET-NORNG-01, E1XD-DI-NONE-01, E1XD-STUB-NONE-01)

```bash
# No non-deterministic primitives anywhere in domain
grep -rEn 'Guid\.NewGuid|DateTime\.(Now|UtcNow)|DateTimeOffset\.(Now|UtcNow)|new Random|Random\.Shared|RandomNumberGenerator\.GetBytes|Environment\.TickCount|Stopwatch\.GetTimestamp' \
  src/domain/{cls}-system/ \
  && echo "FAIL E1XD-DET-NORNG-01"

# No DI imports in domain
grep -rEn 'using Microsoft\.Extensions\.DependencyInjection' src/domain/{cls}-system/ \
  && echo "FAIL E1XD-DI-NONE-01"

# No stubs
grep -rEn 'NotImplementedException|// TODO|// FIXME|// HACK' src/domain/{cls}-system/ \
  && echo "FAIL E1XD-STUB-NONE-01"

# No empty method bodies (heuristic — methods with only a brace pair)
grep -rBn1 -A1 'public void \w+\(.*\)' src/domain/{cls}-system/ \
  | grep -B1 -A1 '^\s*{\s*}\s*$' \
  && echo "REVIEW E1XD-STUB-NONE-01 — manual verify no empty method bodies"
```

---

## Pass criteria

A vertical classification passes the E1 → EX Domain Conformance audit when:

- **Zero S0 findings** across all BCs in the classification.
- **Zero S1 findings** across all BCs in the classification.
- S2 findings are acceptable but captured under `claude/new-rules/{YYYYMMDD-HHMMSS}-domain.md` per CLAUDE.md $1c.

## Fail criteria

Any S0 finding halts execution per CLAUDE.md $16 (violation severity). Block merge until remediated.

Any S1 finding blocks D1 (Partial) promotion; the BC cannot progress to sections 7–18 until S1s are resolved.

## Output format

Audit output follows the canonical pattern:

```
E1XD-AUDIT-RUN:
  timestamp: <YYYY-MM-DDTHH:MM:SSZ>
  classifications_scanned: [<list>]
  bcs_scanned: <count>
  findings:
    - rule: <E1XD-*>
      severity: <S0|S1|S2|S3>
      file: <path>
      line: <n>
      evidence: <grep match or file content>
      remediation: <instruction>
  s0_count: <n>
  s1_count: <n>
  s2_count: <n>
  verdict: <PASS | FAIL>
  new_rules_captured: [<files under claude/new-rules/>]
```

## Related audits

- [`constitutional.audit.md`](constitutional.audit.md) — determinism, HSID, replay, hash.
- [`domain.audit.md`](domain.audit.md) — WBSM v3 core purity, nesting, naming, DTO, behavioral.
- [`runtime.audit.md`](runtime.audit.md) — no-dead-code, stub-detection, pipeline order.
- [`infrastructure.audit.md`](infrastructure.audit.md) — composition, Kafka, config.

When a BC passes ALL four plus this audit, its domain portion (sections 1–6) is ready for promotion to D1 (Partial). Sections 7–18 are validated by subsequent audits (not yet created — will be added as E1 → EX v1.1+ audits when the engine / runtime / API / projection prompts are executed).

## Version lock

- **Audit version:** 1.0
- **Matches template version:** E1 → EX Delivery Pattern v1.0 (locked 2026-04-19)
- **Review cadence:** update when template moves to v1.1 (addition of `06-migration-from-non-canonical.md`) or v1.2.
