# New Rules Capture — Domain Model Remediation 2026-04-22

**Source:** Full domain model remediation execution (Phase 1–6)  
**Prompt:** `claude/project-prompts/20260422-143131-domain-doctrine-domain-model-remediation.md`  
**Severity:** Mixed (see individual rules)

---

## FINDING-01: domain.guard.md Enumerates 12 Systems — Codebase Has 14

**CLASSIFICATION:** domain  
**SOURCE:** domain.guard.md § "Canonical Super-Classification Systems"  
**DESCRIPTION:** The domain guard explicitly states "exactly 12 root systems plus shared-kernel" and lists them. However, the codebase contains 14 root systems: the 12 listed plus `content-system` and `integration-system`. Both are valid, inhabited canonical systems. The guard is stale.  
**PROPOSED_RULE:** Domain guard must be updated to enumerate all 14 root systems. `content-system` (artifact/lifecycle) and `integration-system` (external boundary) must be added to the canonical system table with their doctrinal roles, dependency rules, and content constraints.  
**SEVERITY:** S1 — architectural; guard is the source of truth for system topology, and an incorrect count misleads audits and future extension decisions.

---

## FINDING-02: economic-system/enforcement Was Misclassified as Monetary Truth

**CLASSIFICATION:** domain  
**SOURCE:** Discovered during Phase 2A remediation  
**DESCRIPTION:** `economic-system/enforcement` (lock, restriction, sanction, escalation, rule, violation) was placed in `economic-system` because enforcement affects economic subjects. However, enforcement is a governance constraint application — it imposes system-level restrictions — not a monetary/accounting operation. It owns no ledger entries, no value movements, and no monetary state.  
**PROPOSED_RULE:** Enforcement (constraint application on entities) belongs in `control-system`. The economic-system classification must restrict to: capital, exchange, ledger, reconciliation, revenue, subject, transaction, vault. Any context that applies governance constraints (lock, restrict, sanction) belongs in `control-system/enforcement` regardless of the subject domain.  
**SEVERITY:** S1 — structural boundary violation corrected.  
**STATUS:** RESOLVED — enforcement moved to `control-system/enforcement` in this remediation.

---

## FINDING-03: structural-system/humancapital Contained Business and Evaluative Domains

**CLASSIFICATION:** domain  
**SOURCE:** Discovered during Phase 2B remediation  
**DESCRIPTION:** `structural-system/humancapital` contained: incentive, stewardship, sponsorship, workforce (business behavioral concepts), performance and reputation (evaluative concepts), sanction (enforcement). None of these represent structural binding truth. Structural truth = topology, binding, reference. Business behavior, evaluation, and enforcement are separate truth types.  
**PROPOSED_RULE:** `structural-system/humancapital` must contain only structural participant binding concepts: participant, operator, assignment, eligibility, governance. All behavioral, evaluative, and enforcement sub-domains are violations of structural classification purity.  
**SEVERITY:** S1 — structural system contamination corrected.  
**STATUS:** RESOLVED — behavioral domains moved to `business-system/workforce`, evaluative to `decision-system/evaluation`, enforcement to `control-system/enforcement`.

---

## FINDING-04: Two Orchestration Contexts Created Doctrinal Ambiguity

**CLASSIFICATION:** domain  
**SOURCE:** Discovered during Phase 2C remediation  
**DESCRIPTION:** `control-system/orchestration` (execution-control, schedule-control, system-job) and `orchestration-system` (full workflow execution) co-existed, creating an ambiguous dual orchestration model. The naming conflict made it impossible to determine the canonical orchestration authority.  
**PROPOSED_RULE:** Exactly one canonical execution coordination system may exist: `orchestration-system`. `control-system` must not contain an `orchestration` context. System-level administrative scheduling and job control belongs in `control-system/scheduling` — a distinctly named context that does not conflict with business workflow orchestration.  
**SEVERITY:** S1 — architectural ambiguity corrected.  
**STATUS:** RESOLVED — `control-system/orchestration` renamed to `control-system/scheduling` (Option A).

---

## FINDING-05: operational-system Contained Infrastructure Concerns and Demo Domains

**CLASSIFICATION:** domain  
**SOURCE:** Discovered during Phase 2D remediation  
**DESCRIPTION:** `operational-system/deployment` (infrastructure deployment lifecycle) and `operational-system/sandbox/{kanban,todo}` (demo/tutorial aggregates) existed in a production domain layer. Infrastructure deployment is not a domain concept. Demo aggregates (kanban board, todo list) serve no doctrinal purpose in production domain models.  
**PROPOSED_RULE:** The domain layer must not contain infrastructure deployment contexts. The domain layer must not contain demo or tutorial aggregates. `operational-system` must contain only real operational use-case domains.  
**SEVERITY:** S2 — structural pollution. Demo domains removed. Deployment emergency → incident-response. Deployment activation → activation context (as deployment-activation).  
**STATUS:** RESOLVED — deployment removed, sandbox removed, emergency moved to incident-response, activation consolidated.

---

## FINDING-06: operational-system Missing Mandatory Functional Domains

**CLASSIFICATION:** domain  
**SOURCE:** Phase 2D remediation — mandatory addition  
**DESCRIPTION:** `operational-system` had no provisioning, onboarding, fulfillment, service-activation, or operator-workflow contexts. These are real operational use-case domains required for a functional platform. Without them, no operational use-case can be executed within the canonical boundary.  
**PROPOSED_RULE:** `operational-system` must contain at minimum: provisioning, onboarding, fulfillment, service-activation, and operator-workflow contexts. Each must follow the standard CONTEXT/DOMAIN/artifact structure with D0 scaffolding minimum.  
**SEVERITY:** S2 — functional gap corrected.  
**STATUS:** RESOLVED — all five contexts added as D0 scaffolds.

---

## FINDING-07: structural-system/structure Missing master-data Context

**CLASSIFICATION:** domain  
**SOURCE:** Phase 3 — structural binding completion  
**DESCRIPTION:** `structural-system/structure` provides hierarchy-definition, topology-definition, classification, type-definition, reference-vocabularies, and relationship-rules — but no canonical master-data binding interface. Without master-data, other systems have no reference truth anchor point.  
**PROPOSED_RULE:** `structural-system/structure` must include a `master-data` domain providing the reference truth interface for all systems that bind to structural master records. This is the canonical source-of-truth anchor for structural binding.  
**SEVERITY:** S2 — structural gap corrected.  
**STATUS:** RESOLVED — master-data scaffold added.

---

## FINDING-08: incident Context Duplication in operational-system

**CLASSIFICATION:** domain  
**SOURCE:** Phase 2D audit  
**DESCRIPTION:** Two incident-related contexts existed simultaneously: `operational-system/incident` (containing a `response` domain) and `operational-system/incident-response` (containing an `incident` domain). This is a structural inversion — the parent/child relationship was inverted across two separate contexts.  
**PROPOSED_RULE:** Incident management must be unified under a single context: `operational-system/incident-response` with sub-domains `incident` and `response`.  
**SEVERITY:** S2 — structural drift corrected.  
**STATUS:** RESOLVED — consolidated to `incident-response`.

---

## FINDING-09: economic-system/compliance and economic-system/risk Were Evaluative Not Monetary

**CLASSIFICATION:** domain  
**SOURCE:** Phase 2A remediation  
**DESCRIPTION:** Financial compliance auditing and financial risk exposure analysis were placed in `economic-system`. These concepts evaluate economic behavior (is it compliant? what is the risk?) but do not execute monetary operations. Evaluation is `decision-system` truth; execution is `economic-system` truth.  
**PROPOSED_RULE:** Compliance and risk assessment of economic operations belong in `decision-system/compliance` and `decision-system/risk` respectively. `economic-system` is restricted to monetary/accounting execution only. Evaluative contexts must always belong to `decision-system` regardless of the subject domain being evaluated.  
**SEVERITY:** S1 — classification boundary violation corrected.  
**STATUS:** RESOLVED — financial-audit → `decision-system/compliance`, financial-exposure → `decision-system/risk`.

---

## FINDING-10: economic-system/routing Was an Operational Use-Case Not Monetary Truth

**CLASSIFICATION:** domain  
**SOURCE:** Phase 2A remediation  
**DESCRIPTION:** Transaction routing (execution path selection, routing path definitions) was placed in `economic-system`. Routing selects which execution path a transaction takes — this is an operational use-case decision, not a monetary/accounting state change. No value is recorded or transferred by the routing act itself.  
**PROPOSED_RULE:** Transaction routing (path selection, execution routing) belongs in `operational-system`. Only monetary execution outcomes belong in `economic-system`.  
**SEVERITY:** S1 — classification boundary violation corrected.  
**STATUS:** RESOLVED — routing moved to `operational-system/routing`.
