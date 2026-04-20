# 05 — Quality Gates (D2 Promotion Checklist)

A vertical bounded system (EX) is **D2 (Active)** only when **every** check in this document passes. D2 is the gate per `domain.guard.md` rule 10 — no engine consumes a BC below D2.

Each check has an **audit command** (grep / count / file existence) so promotion is mechanical, not subjective.

## Gate 1 — Domain Layer (sections 1–6)

| # | Check | Audit command |
|---|---|---|
| 1.1 | Every BC has 7 mandatory artifact subfolders | `ls src/domain/{cls}-system/{ctx}/{dom}/{aggregate,entity,error,event,service,specification,value-object}` |
| 1.2 | Every aggregate is named `{Concept}Aggregate.cs` | `find src/domain/{cls}-system -path "*aggregate*.cs" \| grep -v Aggregate.cs$` returns empty |
| 1.3 | Every event is past-tense `{Subject}{Verb}Event.cs` | `find src/domain/{cls}-system -path "*event*.cs" \| grep -v Event.cs$` returns empty |
| 1.4 | Zero `Guid.NewGuid()` in domain | `grep -rn 'Guid\.NewGuid' src/domain/{cls}-system` returns empty |
| 1.5 | Zero `DateTime.Now` / `DateTime.UtcNow` in domain | `grep -rEn 'DateTime\.(Now\|UtcNow)' src/domain/{cls}-system` returns empty |
| 1.6 | Zero `Random` instantiation in domain | `grep -rn 'new Random' src/domain/{cls}-system` returns empty |
| 1.7 | Zero `Microsoft.Extensions.DependencyInjection` imports | `grep -rn 'Microsoft\.Extensions\.DependencyInjection' src/domain/{cls}-system` returns empty |
| 1.8 | Zero raw BCL exception throws (D-ERR-TYPING-01) | `grep -rEn 'throw new (ArgumentException\|ArgumentNullException\|InvalidOperationException\|NotImplementedException)' src/domain/{cls}-system` returns empty |
| 1.9 | All identifiers use VO types (D-VO-TYPING-01) | `grep -rEn 'public Guid \w+Id\|Guid \w+Id[,)]\|string \w+Id[,)]' src/domain/{cls}-system` returns empty (or each hit has documented justification) |
| 1.10 | Every aggregate has lifecycle-init guard `if (Version >= 0)` | Manual review — every Open/Create/Initialize method checks Version >= 0 |

## Gate 2 — Engine Layer (sections 7–9)

| # | Check | Audit command |
|---|---|---|
| 2.1 | Every command has T1M workflow OR T2E handler | Cross-reference command list against `src/engines/{T1M,T2E}/{cls}/` |
| 2.2 | Zero `NotImplementedException` in engines | `grep -rn 'NotImplementedException' src/engines/{T1M,T2E}/{cls}` returns empty |
| 2.3 | Zero `// TODO` / `// FIXME` / `// HACK` | `grep -rEn '// (TODO\|FIXME\|HACK)' src/engines/{T1M,T2E}/{cls}` returns empty |
| 2.4 | Zero direct DB / Kafka / HTTP calls | `grep -rEn 'DbContext\|NpgsqlConnection\|IProducer<\|HttpClient' src/engines/{T1M,T2E}/{cls}` returns empty |
| 2.5 | Zero engine-to-engine references | `grep -rEn 'using Whycespace\.Engines\.(T0U\|T1M\|T2E\|T3I\|T4A)' src/engines/{T1M,T2E}/{cls}` references only the same tier |
| 2.6 | Every step records OTel metrics | Manual review — every step constructor takes `Meter` and increments counters |
| 2.7 | Every handler returns `RuntimeResult<T>` | `grep -rEn 'public async Task<RuntimeResult<' src/engines/T2E/{cls}` covers all handler entry points |

## Gate 3 — Runtime Wiring (sections 10–11)

| # | Check | Audit command |
|---|---|---|
| 3.1 | Every command resolves through dispatcher | Manual: cross-reference `RuntimeCommandDispatcher` aggregate-id candidates against vertical's aggregate VOs |
| 3.2 | Every event registered in `DomainSchemaCatalog` | `grep -rn 'Register{Vertical}' src/runtime/event-fabric/DomainSchemaCatalog.cs` returns hit; cross-reference event count |
| 3.3 | Every wrapper-struct VO in event payload covered by converter or factory | Manual: for each `record struct X(Y Value)` in domain events, confirm `WrappedPrimitiveValueObjectConverterFactory` matches OR a per-type converter exists in `EventDeserializer.StoredOptions` |
| 3.4 | Composition root in `BootstrapModuleCatalog.All` | `grep -n '{Vertical}CompositionRoot' src/platform/host/composition/BootstrapModuleCatalog.cs` returns hit |
| 3.5 | Kafka topics declared | `ls infrastructure/event-fabric/kafka/topics/{cls}/{ctx}/{dom}/` returns files for every event type |
| 3.6 | Zero `Guid.NewGuid()` in runtime per DET-ADAPTER-01 | `grep -rn 'Guid\.NewGuid' src/runtime/ src/platform/host/adapters/` returns empty |

## Gate 4 — Policy (section 7 — cross-cutting)

| # | Check | Audit command |
|---|---|---|
| 4.1 | Every command has policy ID per PB-01 | Cross-reference command list against `infrastructure/policy/domain/{cls}/{ctx}/{dom}/` |
| 4.2 | Every policy is registered in policy registry per POL-05 | Manual: confirm registry contains every policy ID |
| 4.3 | Default deny — no command without explicit policy passes middleware | Run e2e test that dispatches a command with no policy ID — must fail |
| 4.4 | `policy_decision_hash` and `policy_version` populated on every event row per POL-AUDIT-16 | `SELECT count(*) FROM events WHERE policy_decision_hash IS NULL` = 0 |

## Gate 5 — API + DTO (section 13)

| # | Check | Audit command |
|---|---|---|
| 5.1 | Every command has at least one controller endpoint | Cross-reference command list against `src/platform/api/controllers/{cls}/` |
| 5.2 | Zero placeholder `return Ok();` returns | `grep -rEn 'return Ok\(\);$' src/platform/api/controllers/{cls}/` returns empty |
| 5.3 | DTO naming compliance | `grep -rEn '\b\w+(Dto\|Data\|Info)\b' src/platform/api/controllers/{cls}/` returns empty (DTO-R1..R4) |
| 5.4 | Routes follow `/api/{cls}/{ctx}/{dom}/{action}` | Manual review of `[Route(...)]` attributes |

## Gate 6 — Projections (section 12)

| # | Check | Audit command |
|---|---|---|
| 6.1 | Every event in vertical reaches a projection handler | Cross-reference event list against handler `Apply` switch arms |
| 6.2 | Reducers are pure (no I/O, no async) | `grep -rEn 'async\|await\|HttpClient\|DbContext' src/projections/{cls}/` should only match handler files, never reducers |
| 6.3 | Read models have stable schema (no untyped `JsonElement` fields) | Manual review |

## Gate 7 — Tests (section 16)

| # | Check | Audit command |
|---|---|---|
| 7.1 | Unit test count proportional to BC count | `find tests/unit/{cls}-system -name '*.cs' \| wc -l` ≥ BC count × 1.5 |
| 7.2 | Integration test count proportional to command count | `find tests/integration/{cls}-system -name '*.cs' \| wc -l` ≥ command count × 0.5 |
| 7.3 | E2E test count covers every BC | One folder per BC under `tests/e2e/{cls}/{ctx}/{dom}/` |
| 7.4 | Replay regression test for every aggregate with wrapper-struct VOs in events | Per INV-REPLAY-LOSSLESS-VALUEOBJECT-01; canonical template: `tests/integration/economic-system/vault/account/VaultAccountReplayRegressionTest.cs` |

## Gate 8 — Documentation (section 18)

| # | Check | Audit command |
|---|---|---|
| 8.1 | Every BC has a README under `src/domain/{cls}-system/{ctx}/{dom}/README.md` | `find src/domain/{cls}-system -name README.md` returns one per BC |
| 8.2 | Vertical-level README at `src/domain/{cls}-system/README.md` exists and lists all BCs + their D-level | File existence check |
| 8.3 | Vertical entry in `claude/audits/` with a `*.audit.md` for the vertical's invariants | File existence check |
| 8.4 | Vertical's promotion to D2 captured in `claude/new-rules/{YYYYMMDD-HHMMSS}-promotion.md` per $1c | File existence check |

## Gate 9 — Anti-Drift (cross-cutting)

| # | Check | Audit command |
|---|---|---|
| 9.1 | No new architecture introduced — vertical follows the 18-section template | Manual code review |
| 9.2 | No file moves outside the vertical's classification root | Git diff review |
| 9.3 | No renaming of canonical types from shared kernel | `git diff` shows zero changes under `src/domain/shared-kernel/` |
| 9.4 | All four canonical guards (`constitutional`, `runtime`, `domain`, `infrastructure`) re-pass | Run audit sweep per CLAUDE.md $1b |

## Final D2 Declaration

When **every** check in gates 1–9 passes:

1. Update `claude/audits/{vertical}.audit.md` with the pass evidence.
2. Capture promotion in `claude/new-rules/{YYYYMMDD-HHMMSS}-{vertical}-d2-promotion.md` per $1c.
3. Mark the vertical as D2 in the vertical-level README.
4. Update `claude/project-topics/v2b/phase-2b.md` audit status from 🟡 Partial / ⚪ Not Started to ✅ Done for the vertical's items.

Until D2, the vertical is consumable only by tests — not by other engines, not by the API surface in a production composition.
