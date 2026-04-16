# Whycespace Guard System — Canonical Index

Canonical 4-layer guard model (LOCKED 2026-04-14, `GUARD-LAYER-MODEL-01`).

## Guard Files

- `constitutional.guard.md` — WHYCEPOLICY authority, determinism, deterministic identifiers (HSID), hash determinism, replay determinism.
- `runtime.guard.md` — Execution pipeline & ordering, engine, projection, prompt container, dependency graph & layer boundaries, contracts boundary, **code quality enforcement** (clean-code, no-dead-code, stub-detection), **test & E2E validation**.
- `domain.guard.md` — Business truth: purity, structure, classification-suffix, DTO naming, behavioral, structural, and inlined domain-aligned guards (economic, governance, identity, observability, workflow).
- `infrastructure.guard.md` — Platform, systems, kafka, config safety, composition loader, program composition.

## Rule Ownership Doctrine

- **Constitutional** owns WHYCEPOLICY authority and determinism primitives.
- **Runtime** enforces execution, layer boundaries, code quality, and test validation.
- **Domain** owns business truth (aggregates, invariants, naming).
- **Infrastructure** supports external integration and host composition.

## Subsystem Note

Quality enforcement and Test/E2E validation are **subsystems of Runtime enforcement**, not separate top-level guards. Per `GUARD-LAYER-MODEL-01`:

- No fifth top-level guard file may exist.
- No subdirectories (including any `domain-aligned/`) may exist under `claude/guards/`.
- Domain-aligned guards live as subsections of `domain.guard.md`.

## Notes

- All legacy fragmented guards (32 source files) have been merged into these 4 canonical files.
- No rule duplication permitted across guards.
- WBSM v3 Global Enforcement block (GE-01..05) appears at most once per canonical file.
- The four audit definitions under `claude/audits/canonical/` are 1:1 aligned with these four guards.
