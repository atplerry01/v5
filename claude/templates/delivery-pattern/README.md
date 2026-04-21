---
name: E1 → EX Delivery Pattern
type: canonical-template
version: 1.0
status: locked
classification: shared/orchestration
extracted-from: economic-system (Phase 2 vertical, ~98% complete as of 2026-04-19)
---

# E1 → EX Reusable Delivery Pattern

Canonical template for delivering a vertical bounded system (domain → engine → runtime → API → projection → tests) end-to-end. Extracted from the **proven economic-system implementation** to accelerate Phase 2.5 (structural), 2.6 (content), 2.7 (policy), 2.8 (WhyceID), and 2.9 (WhyceChain) builds.

## Why this exists

Phase 2 closure audit (2026-04-19) found:

- **Item #1 — economic domain canonical definition**: ✅ Done
- **Item #4 — E1 → EX reusable delivery pattern standardization**: ⚪ Not Started

Each subsequent vertical system (2.5–2.9) declares 15–20 sections that mirror the economic build. Without a template, every vertical is hand-crafted — expensive, drift-prone, and slow. This template captures the proven pattern so the next 4 verticals reuse it instead of rediscovering it.

## What "E1 → EX" means

- **E1** — the first proven vertical: economic-system, verified 2026-04-21: **13 contexts** (2nd-level), **42 domain BCs** (3rd-level), **40 aggregates**, **140 domain events**, **41 projection handlers** (40 reducers), **39 controllers**, **107 tests** (20 unit / 34 integration / 53 e2e), **31 rego policies**, **12 kafka topic files**, 0 red flags.
- **EX** — every subsequent vertical (E2 = structural, E3 = content, E4 = business, E5 = operational, and any additional vertical). Each follows the same 18-section skeleton.

> **Terminology convention:** *context* = 2nd-level folder under `src/domain/{classification}-system/` (13 in economic). *Domain BC (bounded context)* = 3rd-level folder (42 in economic). Quality gates that mention "BC count" refer to 3rd-level domain BCs.

## How to use

1. Read [00-section-checklist.md](00-section-checklist.md) — the 18 ordered sections every vertical must implement.
2. For each section, follow the matching skeleton doc (`01` … `05`) which points at the canonical economic exemplar files.
3. Apply [05-quality-gates.md](05-quality-gates.md) before declaring the vertical D2 (Active).

## Files in this template

### Skeleton reference docs

| File | Purpose |
|---|---|
| [00-section-checklist.md](00-section-checklist.md) | The 18 ordered sections, with definition-of-done per section. Also indexes the cross-cutting Cross-System Invariant Layer. |
| [01-domain-skeleton.md](01-domain-skeleton.md) | Domain layer: folders, naming, aggregate/event/VO/error/spec/service patterns. Defines the Cross-System Invariant concept + `invariant/{policy-name}/` folder. |
| [02-engine-skeleton.md](02-engine-skeleton.md) | T1M/T2E engine layer: steps, workflows, pipeline, handlers. Defines cross-system invariant enforcement (T2E inline / T1M step). |
| [03-runtime-wiring.md](03-runtime-wiring.md) | Dispatcher registration, middleware enforcement, composition root entries. Registers domain policies via `AddDomain{Vertical}Invariants()`. |
| [04-api-projection-tests.md](04-api-projection-tests.md) | Controller pattern, projection reducer + handler, three-tier test pattern. |
| [05-quality-gates.md](05-quality-gates.md) | Audit checklist a vertical must pass before D2 (Active). Includes Gate 9 — Cross-System Invariants. |
| [06-infrastructure-contracts.md](06-infrastructure-contracts.md) | Infrastructure ports, delivery contracts, PolicyEvaluator integration, code-policy vs OPA-policy split. |

### Cross-System Invariant Layer (cross-cutting)

The template guarantees **cross-system truth — consistency between all systems** alongside structural, business, content, economic, and operational truth. A cross-system invariant is a **domain-level policy spanning multiple bounded contexts** — pure decision functions living under `src/domain/{classification}-system/invariant/{policy-name}/`, enforced inside the engine before aggregate mutation, registered in the composition root, gated before D2 promotion (Gate 9), and optionally backed by OPA when operator-tunable. Canonical examples: `ContractEconomicPolicy`, `AmendmentContentPolicy`, `DocumentOwnershipPolicy`, `SettlementLedgerPolicy`. See the per-file touch points in `00-section-checklist.md` § Cross-System Invariant Layer.

### Reusable prompt templates

Copy into `claude/project-prompts/{YYYYMMDD-HHMMSS}-{vertical}-delivery.md` per CLAUDE.md $2. Fill placeholders and execute.

| File | Drives | Verified by |
|---|---|---|
| [prompts/e1-ex-delivery-prompt.md](prompts/e1-ex-delivery-prompt.md) | All 18 phases (domain → engine → runtime → infrastructure → API → projections → tests → docs) | [claude/audits/e1-ex-domain.audit.md](../../audits/e1-ex-domain.audit.md) + four canonical guards |

**Single source of truth.** The v2.0 unified prompt supersedes the v1.0 per-section prompts (`domain-implementation-prompt.md`, proposed engine/runtime/api/closure prompts). Scope each execution by filling `{{PHASES_IN_SCOPE}}` — e.g., `1–6` for domain-only delivery to D1, `1–18` for full D2 delivery.

### Companion audits (run per CLAUDE.md $1b)

| File | Validates | Severity gates |
|---|---|---|
| [claude/audits/e1-ex-domain.audit.md](../../audits/e1-ex-domain.audit.md) | Sections 1–6 conformance against [01-domain-skeleton.md](01-domain-skeleton.md) + [05-quality-gates.md](05-quality-gates.md) Gate 1 | S0/S1 block promotion; S2 captured to new-rules |

Future audits (v1.1+):

- `e1-ex-engine.audit.md` — sections 7–9
- `e1-ex-runtime.audit.md` — sections 10–11
- `e1-ex-api-projection-tests.audit.md` — sections 12, 13, 16
- `e1-ex-closure.audit.md` — sections 14, 15, 17, 18 + D2 gate

## Version lock and roadmap

**v1.0 — locked 2026-04-19.** The 7 files in this directory are the canonical reusable template extracted from the economic-system exemplar. They cover 80% of any vertical build: domain, engine, runtime, API, projection, tests, and auditable quality gates.

### Out of scope for v1.0 (will surface as needed)

**v1.1 candidates** — add if/when they surface during a vertical pilot:

1. **06-migration-from-non-canonical.md** — how to lift an existing non-conformant aggregate to `AggregateRoot`, add `Version` retroactively, add `[JsonPropertyName]` without breaking historical events, and backfill `LoadFromHistory`. Relevant the moment a vertical like structural-system (22 BCs with manual event lists) needs promotion.
2. **07-cross-system-events.md** — where shared event contracts live when vertical A emits and vertical B subscribes. Namespace, package, version compatibility. Glossed over in v1.0's section 15.

**v1.2 candidates** — defer until multiple verticals need them:

3. **08-observability-naming.md** — canonical meter names, span names, and log field conventions. v1.0 says "record OTel metrics" without pinning names.
4. **Per-vertical addenda** — content needs storage/streaming/CDN layers; structural needs hierarchy/topology; identity needs auth/sessions/devices. Each EX vertical has unique sections beyond the common 18. Capture as `addenda/{vertical}.md` when the second vertical after economic needs it.

### Permanently out of scope

- Code-generation tooling (`dotnet new` template). Not aligned with the project's hand-curated domain discipline.
- Auto-scaffolding scripts. Same reason.
