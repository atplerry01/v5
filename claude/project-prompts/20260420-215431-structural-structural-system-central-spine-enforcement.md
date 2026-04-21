# TITLE
Structural-System Central-Spine Enforcement Pass (Phase 2.5 Hardening)

## CLASSIFICATION
- **Classification:** structural
- **Context:** structural-system (cross-system enforcement)
- **Domain:** cluster / structure / humancapital (and cross-binding discipline on business, content, economic, operational)

## CONTEXT
The WBSM v3 architecture positions five bounded super-systems:

- `structural-system` — where it belongs (authoritative placement, hierarchy, parent binding)
- `business-system`   — what it means (agreements, offerings, obligations)
- `content-system`    — what content it has (artifacts, lifecycle)
- `economic-system`   — what value/money it carries (vaults, accounts, allocations, revenue)
- `operational-system`— what it does (workflows, use-cases)

Doctrine: `structural-system` is the **central spine**. All other systems MUST reference structural truth via canonical refs and MUST NOT own, duplicate, or invent placement, hierarchy, parent relationships, or cluster/authority/subcluster/SPV/provider/administration concepts.

Canonical cluster model:
```
Cluster
├── ClusterAuthority
├── ClusterProviders
├── ClusterAdministration
├── SubCluster
└── SPVs
```

Canonical separation:
```
structure/*   = definitions (rules, types, topology, reference vocabularies)
cluster/*     = live instances
humancapital/*= workforce placement / role-in-structure
```

## OBJECTIVE
Close the remaining structural gaps and formalize binding contracts so every non-structural system hangs from structural truth rather than inventing its own placement. Deliver a hardening checklist + violations report + phased enforcement plan. Perform only surgical, authorized edits.

## CONSTRAINTS
- CLAUDE.md $5 Anti-Drift: no unilateral architecture inventions. New aggregates, ref types, and cross-system deletions require explicit phase authorization before edit.
- CLAUDE.md $7 Layer Purity: domain has zero external dependencies.
- CLAUDE.md $15 Priority: WHYCEPOLICY > WBSM v3 > Rules > Prompt.
- Guards loaded: `constitutional.guard.md`, `runtime.guard.md`, `domain.guard.md`, `infrastructure.guard.md` (four canonical).
- DS-R3 / DS-R3a: domain topology `classification/context/[domain-group]/domain`.

## EXECUTION STEPS
1. Pre-execution guard load ($1a) — done.
2. Survey A: structural-system completeness (cluster + structure + humancapital coverage).
3. Survey B: cross-system structural leakage (business, content, economic, operational).
4. Produce consolidated violations report.
5. Produce Phase 2.5 Structural Hardening Checklist with phased execution plan.
6. Archive this prompt ($2).
7. HALT before invasive code edits — surface the phase plan, await user authorization.
8. Post-execution audit sweep ($1b) + drift capture ($1c).

## OUTPUT FORMAT
- `claude/project-topics/v2b/phase-2.5-structural-hardening-checklist.md` — authoritative Phase 2.5 plan.
- `claude/project-topics/v2b/phase-2.5-structural-violations-report.md` — consolidated findings with file + line + severity.
- Inline: phase-plan summary, explicit asks for phase-1 authorization.

## VALIDATION CRITERIA
- Every finding cites file path + line + severity (S0 / S1 / S2 / S3).
- Hardening checklist is phase-ordered and non-overlapping.
- No unauthorized code edits were performed.
- All guard rules loaded; no guard violation in audit sweep.
- New drift captures (if any) written under `claude/new-rules/` per $1c.
