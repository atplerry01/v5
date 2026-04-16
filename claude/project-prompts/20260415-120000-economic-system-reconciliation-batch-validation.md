TITLE: End-to-End Validation & Certification — Domain Batch (Generic Canonical) — economic-system/reconciliation

CLASSIFICATION: economic-system
CONTEXT: reconciliation
DOMAIN GROUP: reconciliation
DOMAINS: discrepancy, process

CONTEXT:
Full-system certification pass per validation-prompt.md against the WBSM v3 canonical execution pipeline. Invoked from CLAUDE.md $1 (Execution Flow) with guards loaded per $1a and audits planned per $1b.

OBJECTIVE:
Perform a SYSTEM CERTIFICATION PASS across the 15 validation sections: scope/structure, domain model (E1-S4), command (E2), query (E3), engine handler (E4-T2E), policy (E5), event fabric (E6), Postgres, Redis, workflow (E9), API (E8), E2E (E12), observability (E10), security (E11).

CONSTRAINTS:
- STRICT mode: no assumed correctness; every layer verified with evidence.
- WHYCEPOLICY + determinism + HSID + chain-anchor rules from constitutional.guard.md are binding.
- Infrastructure rules (outbox, dual-topic, canonical naming) from infrastructure.guard.md.

EXECUTION STEPS:
1. Load all `claude/guards/*.guard.md` per $1a.
2. Enumerate `src/domain/economic-system/reconciliation/{discrepancy,process}/**`.
3. Search for command/query/handler/policy/controller/projection artifacts across the repo.
4. Grep determinism block list against the domain batch scope.
5. Produce certification output.
6. Run `claude/audits/*.audit.md` post-sweep per $1b.

OUTPUT FORMAT:
Per `validation-prompt.md §16` — overall status, per-domain status, infrastructure status, critical failures, non-critical gaps, evidence summary, certification decision.

VALIDATION CRITERIA:
FAIL on any of: determinism breach, policy not enforced, events not persisted, kafka not wired, projections missing, per MANDATORY FAILURE RULE.