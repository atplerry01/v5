# Domain Classification Doctrine Table (LOCKED 2026-04-22)

> Output of Phase 1 — Doctrine Stabilization  
> Part of full domain model remediation (project-prompts/20260422-143131-domain-doctrine-domain-model-remediation.md)  
> Status: AUTHORITATIVE — supersedes prior implicit assignments  

---

## 14-Classification Doctrine

### 1. `core-system` — Universal Primitives (TRIAD Layer 1)

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Universal primitive types. The immutable atomic substrate of the entire platform. |
| **OWNS** | `temporal` (time primitives), `ordering` (sequence/versioning), `identifier` (domain ID types) |
| **REFERENCES** | Nothing — zero dependencies |
| **MUST NOT OWN** | Aggregates with lifecycle, state transitions, services, business logic, behavior of any kind |
| **Truth type** | Definitional / atomic |
| **Contexts** | identifier, ordering, temporal |

**Constraint:** If a concept requires state, events, or lifecycle — it does not belong here.

---

### 2. `platform-system` — Communication Contracts (TRIAD Layer 2)

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Messaging infrastructure primitives. Defines the shape of all cross-system communication without any business semantics. |
| **OWNS** | `command` (command envelope model), `event` (event envelope model), `routing` (message routing model), `schema` (schema contracts) |
| **REFERENCES** | `core-system` only |
| **MUST NOT OWN** | Business semantics, policy logic, authorization logic, domain-specific naming, aggregate behavior |
| **Truth type** | Structural / messaging |
| **Contexts** | command, event, routing, schema |

**Constraint:** If a concept carries business meaning — it does not belong here.

---

### 3. `control-system` — System Governance (TRIAD Layer 3)

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Administrative governance of the platform itself. Manages system-level constraints, access control, audit, configuration, and system scheduling. |
| **OWNS** | `access-control` (authorization/permission/role), `audit` (system audit trail), `configuration` (system config lifecycle), `observability` (system health/alert/trace/metric), `scheduling` (system job/schedule control — renamed from orchestration), `system-policy` (policy decision/definition/enforcement at system level), `enforcement` (economic constraint application — MOVED IN from economic-system) |
| **REFERENCES** | `core-system`, `platform-system` only (per Triad Dependency Rules) |
| **MUST NOT OWN** | Business workflows, domain-specific aggregates, messaging constructs, business workflow orchestration |
| **Truth type** | Governance / constraint |
| **Contexts (after remediation)** | access-control, audit, configuration, observability, scheduling, system-policy, enforcement |

**Conflict resolution:** The `orchestration` context is renamed to `scheduling` to eliminate confusion with `orchestration-system`. The `enforcement` context moves from `economic-system` since enforcement is a governance constraint applied by the system, not a monetary/accounting truth.

---

### 4. `constitutional-system` — Meta-Governance / Rule Definition

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | The meta-governance layer. Owns the definition of policy, the immutable chain (WhyceChain), and the governance rule lifecycle. |
| **OWNS** | `policy` (access, constraint, enforcement, jurisdiction, registry, rule, scope, version, violation), `chain` (ledger, immutable audit chain) |
| **REFERENCES** | `structural-system` (for scope binding), `core-system` |
| **MUST NOT OWN** | Business logic, economic rules, domain-specific workflows |
| **Truth type** | Constitutional / rule definition |
| **Contexts** | chain, policy |

---

### 5. `decision-system` — Evaluation / Decision Truth

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | The evaluation layer. Produces structured decisions from evidence. Owns audit evaluation, compliance assessment, governance decisions, and risk evaluation. |
| **OWNS** | `audit` (audit-case, audit-log, evidence-audit, finding, remediation, access), `compliance` (attestation, compliance-case, filing, jurisdiction, obligation, regulation + financial-audit MOVED IN), `governance` (approval, appeal, committee, delegation, dispute, mandate, proposal, resolution, vote, etc.), `risk` (assessment, alert, control, exception, exposure, incident-risk, mitigation, rating, review, threshold + financial-exposure MOVED IN), `evaluation` (performance, reputation — MOVED IN from structural) |
| **REFERENCES** | `structural-system` (for actor/participant identity), `business-system` (for context), `economic-system` (for financial evidence) |
| **MUST NOT OWN** | Enforcement actions (→ `control-system`), economic execution (→ `economic-system`), primary operational workflows (→ `operational-system`) |
| **Truth type** | Evaluative / decision |
| **Contexts (after remediation)** | audit, compliance, governance, risk, evaluation |

**Conflict resolution:** `economic-system/compliance` and `economic-system/risk` move here because compliance and risk are evaluation truth, not monetary/accounting truth. They evaluate economic behavior but do not execute economic operations.

---

### 6. `intelligence-system` — Analytics / Insight / Derived Truth

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Derived and analytical truth. Produces insights, forecasts, simulations, and recommendations from primary truth held by other systems. |
| **OWNS** | `capacity` (demand, supply, constraint, utilization, forecast), `cost` (benchmark, driver, model, structure, variance), `economic` (analysis, anomaly, forecast, optimization, simulation, integrity, kernel, autonomy), `estimation` (cost-estimate, price-estimate, adjustment-factor, benchmark, demand-supply, regional-index), `experiment` (cohort, hypothesis, variant, result-analysis), `geo` (distance, geofence, proximity, region-mapping, routing, geo-index), `identity` (identity-intelligence), `index` (cost-index, performance-index, price-index, regional-index, risk-index), `knowledge` (article, answer, ontology, reference, taxonomy), `observability` (alert, chain-monitor, diagnostic, health, log, metric, trace), `planning` (plan, capacity-plan, scenario-plan, schedule-plan, target), `relationship` (affiliation, graph, influence, linkage, trust-network), `search` (index, query, ranking, result, synonym), `simulation` (assumption, comparison, forecast, model, optimization, outcome, recommendation, scenario, stress-test) |
| **REFERENCES** | `economic-system` (for economic data), `structural-system` (for topology), `business-system` (for business context), `decision-system` (for risk/compliance data) |
| **MUST NOT OWN** | Primary truth of any kind — only derived/analytical truth; execution; enforcement |
| **Truth type** | Derived / analytical |
| **Contexts** | capacity, cost, economic, estimation, experiment, geo, identity, index, knowledge, observability, planning, relationship, search, simulation |

**Note:** `intelligence-system/observability` is distinct from `control-system/observability`. Intelligence observability = analytical health insights (cross-system derived metrics, chain monitoring). Control observability = system-level alert/trace/metric aggregates.

---

### 7. `integration-system` — External Boundary / System Interaction

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | The external integration boundary. Owns outbound effects and integration contracts. The exclusive point of interaction with external systems. |
| **OWNS** | `outbound-effect` (side-effect envelope for external interactions) |
| **REFERENCES** | `platform-system` (for messaging contracts) |
| **MUST NOT OWN** | Business logic, domain state, internal workflows, data transformation beyond contract mapping |
| **Truth type** | Boundary / integration |
| **Contexts** | outbound-effect |

**Note:** `integration-system` is a recognized canonical system missing from the current domain.guard.md enumeration. This is captured as new-rules drift.

---

### 8. `orchestration-system` — Execution Coordination (CANONICAL — Option A)

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Business workflow execution coordination. Owns the full lifecycle of workflow definition, execution, routing, compensation, and state management. |
| **OWNS** | `workflow` (assignment, checkpoint, compensation, definition, escalation, execution, instance, queue, route, stage, step, template, transition) |
| **REFERENCES** | `business-system` (for process context), `operational-system` (for use-case binding), `structural-system` (for topology) |
| **MUST NOT OWN** | System-level scheduling (→ `control-system/scheduling`), business rules (→ `business-system`), infrastructure scheduling |
| **Truth type** | Process / execution state |
| **Contexts** | workflow |

**Conflict resolution (OPTION A SELECTED):** `orchestration-system` is the canonical execution coordination system. `control-system/orchestration` is renamed to `control-system/scheduling` to eliminate the naming conflict. The domains within (execution-control, schedule-control, system-job) govern system-level administrative scheduling, not business workflow execution. NO HYBRID.

---

### 9. `trust-system` — Identity, Trust, Authority Semantics

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Identity and trust authority. Owns the definition, verification, and lifecycle of identities, access grants, credentials, sessions, and trust scoring. |
| **OWNS** | `identity` (consent, credential, device, federation, identity, identity-graph, profile, registry, service-identity, trust, verification), `access` (authorization, grant, permission, request, role, session) |
| **REFERENCES** | `structural-system` (for participant/operator binding), `constitutional-system` (for policy scope) |
| **MUST NOT OWN** | Business authorization (→ `control-system/access-control`), economic identity (→ `economic-system/subject`), structural membership roles (→ `structural-system`) |
| **Truth type** | Identity / trust |
| **Contexts** | access, identity |

**Conflict resolution:** `control-system/access-control` owns the system-level RBAC (authorization/permission/role as system governance). `trust-system/access` owns the identity-layer access semantics (grant, session, request, authorization as trust-domain artifacts). These are distinct: control-system governs WHAT the system allows; trust-system governs WHO the actor is.

---

### 10. `business-system` — Business Meaning

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Business domain truth. Owns the commercial and operational meaning of business entities: agreements, customers, entitlements, offerings, orders, pricing, providers, services, and (MOVED IN) workforce behavioral concepts. |
| **OWNS** | `agreement` (change-control, commitment, party-governance), `customer` (identity-and-profile, segmentation-and-lifecycle), `entitlement` (eligibility-and-grant, usage-control), `offering` (catalog-core, commercial-shape), `order` (order-change, order-core), `pricing` (price-adjustment, pricing-execution, pricing-structure), `provider` (provider-core, provider-governance, provider-scope), `service` (service-constraint, service-core), `workforce` (MOVED IN: incentive, stewardship, sponsorship, workforce) |
| **REFERENCES** | `structural-system` (for topology binding), `economic-system` (for pricing/billing execution), `trust-system` (for identity) |
| **MUST NOT OWN** | Economic execution (→ `economic-system`), structural topology (→ `structural-system`), compliance/risk evaluation (→ `decision-system`), enforcement (→ `control-system`) |
| **Truth type** | Business / commercial |
| **Contexts (after remediation)** | agreement, customer, entitlement, offering, order, pricing, provider, service, workforce |

---

### 11. `economic-system` — Monetary / Accounting

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Financial execution and accounting truth. Owns the pure monetary operations: ledger, capital management, vault, transactions, revenue, exchange, settlement, and reconciliation. |
| **OWNS** | `capital` (account, allocation, asset, pool, reserve, vault), `exchange` (fx, rate), `ledger` (entry, journal, ledger, obligation, treasury), `reconciliation` (discrepancy, process), `revenue` (contract, distribution, payout, pricing, revenue), `subject` (subject — economic actor), `transaction` (charge, expense, instruction, limit, settlement, transaction, wallet), `vault` (account, metrics, slice) |
| **REFERENCES** | `structural-system` (for topology binding), `trust-system` (for actor identity), `business-system` (for commercial context/contracts) |
| **MUST NOT OWN** | Compliance rules (→ `decision-system`), risk evaluation (→ `decision-system`), enforcement authority (→ `control-system`), routing logic as policy (→ `operational-system`) |
| **Truth type** | Monetary / accounting |
| **Contexts (after remediation)** | capital, exchange, ledger, reconciliation, revenue, subject, transaction, vault |

**Removed contexts:** compliance (→ decision-system), enforcement (→ control-system), risk (→ decision-system), routing (→ operational-system)

---

### 12. `structural-system` — Structural / Topology / Reference Truth

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | The canonical reference and topology layer. Owns the binding definitions of clusters, hierarchies, topologies, classification systems, and reference vocabularies. All other systems bind to structural identity; structural does not bind to them. |
| **OWNS** | `cluster` (administration, authority, cluster, lifecycle, provider, spv, subcluster, topology), `contracts` (references — binding interface for how systems reference structural truth), `structure` (classification, hierarchy-definition, reference-vocabularies, relationship-rules, topology-definition, type-definition, master-data ADDED), `humancapital` (REDUCED: assignment, eligibility, governance, operator, participant only — behavioral domains moved out) |
| **REFERENCES** | `core-system` only (as reference primitives) |
| **MUST NOT OWN** | Business behavior (incentive, reputation, stewardship, sponsorship, workforce → `business-system`), performance evaluation (→ `decision-system/evaluation`), sanction enforcement (→ `control-system/enforcement`) |
| **Truth type** | Structural / topological / reference |
| **Contexts (after remediation)** | cluster, contracts, humancapital (reduced), structure |

**Conflict resolution:** `structural-system/humancapital` retains only the structural binding concepts: who participates (participant), who operates (operator), how they're assigned (assignment), eligibility criteria (eligibility), and structural governance rules (governance). Behavioral and economic concepts are removed.

---

### 13. `operational-system` — Execution / Use-Case

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Operational use-case execution layer. Owns the actual operational workflows that span multiple systems: provisioning, onboarding, fulfillment, service activation, operator workflows, and incident response. |
| **OWNS** | `activation` (service activation — consolidated), `incident-response` (incident, response — consolidated), `provisioning` (ADDED: provisioning workflows), `onboarding` (ADDED: participant/operator onboarding), `fulfillment` (ADDED: fulfillment execution), `service-activation` (ADDED: real service activation use-cases), `operator-workflow` (ADDED: operator operational workflows), `routing` (MOVED IN: execution, path — economic transaction routing as operational execution) |
| **REFERENCES** | `business-system` (for use-case context), `structural-system` (for topology), `orchestration-system` (for workflow execution), `economic-system` (for financial operations) |
| **MUST NOT OWN** | Infrastructure deployment (→ infrastructure), demo/sandbox domains, business logic, policy definitions |
| **Truth type** | Operational / use-case |
| **Contexts (after remediation)** | activation, incident-response, routing, provisioning, onboarding, fulfillment, service-activation, operator-workflow |

**Removed:** deployment (infrastructure), sandbox/kanban (demo), sandbox/todo (demo), duplicate incident context

---

### 14. `content-system` — Artifact / Lifecycle

| Attribute | Definition |
|-----------|-----------|
| **Doctrinal role** | Content artifact ownership and lifecycle management. Owns documents, media, and streaming content in their production and lifecycle phases. |
| **OWNS** | `document` (document context domains), `media` (media context domains), `streaming` (streaming content domains) |
| **REFERENCES** | `structural-system` (for ownership binding), `trust-system` (for access control) |
| **MUST NOT OWN** | Business logic, economic concerns, structural topology, system governance |
| **Truth type** | Content / artifact |
| **Contexts** | document, media, streaming |

**Note:** `content-system` is a recognized canonical system missing from the current domain.guard.md enumeration (which lists 12 systems). This is captured as drift.

---

## Conflict Resolution Decisions

### CR-1: control-system vs orchestration-system
**Decision:** OPTION A — `orchestration-system` is canonical execution coordination. `control-system/orchestration` context renamed to `control-system/scheduling`. No overlap, no hybrid.
- `control-system/scheduling` = system job scheduling, execution-control, schedule-control (administrative)
- `orchestration-system` = business workflow full lifecycle (definition through compensation)

### CR-2: economic-system/enforcement vs control-system vs constitutional-system
**Decision:** → `control-system/enforcement`  
Rationale: Enforcement (lock, restriction, sanction, escalation, rule, violation) applies governance constraints on economic subjects. This is system governance applied to economic behavior, not monetary/accounting truth. Constitutional owns *policy definition*; control owns *constraint application*.

### CR-3: economic-system/compliance and economic-system/risk → decision-system
**Decision:** Both move to `decision-system`  
Rationale: Compliance and risk are evaluation truths — they assess economic behavior against criteria and produce decisions. They do not execute economic operations. Financial compliance audit → `decision-system/compliance`; financial risk exposure → `decision-system/risk`.

### CR-4: economic-system/routing → operational-system
**Decision:** → `operational-system/routing`  
Rationale: Transaction routing is an operational use-case — it selects execution paths for transactions. It is not monetary/accounting truth (doesn't record value) and not messaging routing (platform-system). It is the operational decision of how to execute a payment.

### CR-5: structural-system/humancapital — behavioral vs structural domains
**Decision:** Structural binding concepts (participant, operator, assignment, eligibility, governance) STAY. Behavioral/economic concepts (incentive, stewardship, sponsorship, workforce) → `business-system/workforce`. Evaluative concepts (performance, reputation) → `decision-system/evaluation`. Enforcement concepts (sanction) → `control-system/enforcement` (merged with economic enforcement).

### CR-6: economic/control/decision — economic actor (subject)
**Decision:** `economic-system/subject` STAYS. The economic subject is the entity within the economic system that holds accounts and participates in transactions. This is economic truth, not structural truth. Structural gives the topology binding reference; economic owns the subject as an economic participant.

### CR-7: control-system/audit vs decision-system/audit
**Decision:** Both exist and serve distinct roles.
- `control-system/audit` = system audit trail (audit-log, audit-record) — system-level event capture
- `decision-system/audit` = audit evaluation (audit-case, finding, remediation, evidence) — produces audit decisions

### CR-8: content-system and integration-system missing from domain guard
**Decision:** These are legitimate canonical systems. domain.guard.md must be updated to enumerate 14 root systems (not 12). Captured as new-rules drift (domain guard update required).
