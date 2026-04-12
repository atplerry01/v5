# PHASE 3 -- SYSTEM INTEGRATION & FULL ACTIVATION v2

## SCOPE: E17 - E20 + EX (System-Level Activation)

## PRE-REQ: Phase 1 + Phase 2 fully complete

## VERSION: 2.0

---

# OBJECTIVE

Transform the system from a working, observable platform into a fully
integrated economic and institutional system capable of real-world
execution at scale.

---

# IMPLEMENTATION FLOW

```
E17 Economic Integration
  E18 Structural Integration
  E19 SPV Integration
  E20 Workforce (CWG) Integration
  EX System-Level Activation
  PHASE 3 PROOF
```

---

# E17 -- ECONOMIC INTEGRATION

## Goal

Connect domain flows to real money, capital, and financial movement.

## Implementation

### 1. Capital System

* Capital allocation to domains/SPVs
* Vault structures (WhyceWallet / WhyceBank)
* Funding flows

### 2. Ledger Integration

* Double-entry ledger linkage
* All economic events recorded as ledger entries
* Balance = fold(events)

### 3. Transaction Flows

* Atomic transaction execution
* Multi-step financial operations
* Replay-safe financial behaviour

### 4. Settlement Integration

* External settlement references
* Bank/payment system integration
* Irreversibility guarantees

## Implementation Notes (from Phase 1 learnings)

The economic domain already exists at src/domain/economic-system/. During
Kanban validation, 7 pre-existing compilation errors were fixed in this domain
(CS0111 duplicate constructors on value object IDs, CS0507 access modifier
mismatches on aggregate Apply/EnsureInvariants overrides). These have been
corrected and the economic domain now compiles cleanly.

Each economic subdomain (ledger/entry, ledger/journal, ledger/obligation,
ledger/settlement, ledger/treasury, capital/pool, capital/vault) must follow
the same E1-E8 implementation path validated with Kanban:

* Commands with Id as first positional parameter
* Schema modules with FlexibleIntConverter/FlexibleGuidConverter support
* Dedicated Kafka topics per domain
* OPA policies per domain
* Projection DB schemas per domain
* Bootstrap modules registered in BootstrapModuleCatalog
* Hosted service registration via AddSingleton<IHostedService>

## Where

```
src/domain/economic-system/**
src/engines/T2E/economic/**
src/shared/contracts/economic/**
src/projections/economic/**
```

---

# E18 -- STRUCTURAL INTEGRATION

## Goal

Bind domain flows into the Whycespace structural hierarchy.

## Implementation

### 1. Holding / Global Structure

* Holding-level governance
* Global coordination

### 2. Cluster Integration

* Domain flows mapped to clusters
* Authority/SubCluster enforcement

### 3. Authority Binding

* Each domain flow linked to a governing authority
* Ownership and control defined

### 4. Lifecycle Integration

* Domain flows align with structural lifecycle states

## Where

```
src/domain/structural-system/**
src/systems/structural/**
```

---

# E19 -- SPV INTEGRATION

## Goal

Convert execution into real economic business units (SPVs).

## Implementation

### 1. SPV Creation & Lifecycle

* SPV entity creation
* Lifecycle: draft -> active -> suspended -> closed

### 2. Operational Binding

* Domain flows executed under SPV ownership
* Revenue attribution to SPVs

### 3. Service Provider Integration

* External/internal service providers bound to SPVs
* Execution responsibility defined

### 4. Continuity & Replaceability

* Operator replaceability
* SPV continuity rules

## Where

```
src/domain/structural-system/spv/
src/systems/spv/
```

---

# E20 -- WORKFORCE (CWG) INTEGRATION

## Goal

Introduce human/system participation into execution.

## Implementation

### 1. Workforce Assignment

* Assign actors to roles/tasks
* Bind users to workflows

### 2. Participation Governance

* Role-based participation
* Permission enforcement
* Behaviour tracking

### 3. Performance Tracking

* Contribution metrics
* Output measurement
* Reward linkage

### 4. CWG Economic Linkage

* Profit routing
* Vesting logic
* Clawback enforcement

## Where

```
src/domain/trust-system/cwg/
src/systems/workforce/
```

---

# EX -- SYSTEM-LEVEL ACTIVATION

## Goal

Activate multi-domain, multi-system execution.

## Implementation

* Cross-domain workflows (economic + structural + operational)
* Multi-step orchestration across systems
* Real-world use cases

## Example Flow

```
Investor capital -> Capital system (E17)
  -> Allocated to SPV (E18/E19)
  -> Workforce executes tasks (E20)
  -> Domain operations run (Phase 1 system)
  -> Revenue generated -> Ledger updated -> Settlement executed
  -> Projections updated -> API reflects full system state
```

---

# PHASE 3 PROOF

## Required Evidence

### Economic
* [ ] Capital flows executed
* [ ] Ledger reflects transactions
* [ ] Settlement recorded
* [ ] All economic events in EventStore + WhyceChain

### Structural
* [ ] Domain flows tied to clusters/SPVs
* [ ] Authority enforcement visible
* [ ] Structural hierarchy queryable via projections

### SPV
* [ ] SPV lifecycle functioning
* [ ] Revenue attributed to SPVs
* [ ] Ownership chain traceable

### Workforce
* [ ] Actors assigned to workflows
* [ ] Performance tracked
* [ ] Participation enforced

### Cross-System
* [ ] Multi-domain workflows succeed end-to-end
* [ ] Economic + structural + operational layers interact correctly
* [ ] Full audit trail across all systems via WhyceChain

---

# CRITICAL RULE

Do NOT start Phase 3 until Phase 1 and Phase 2 are fully stable.
Financial inconsistencies, structural misalignment, and workforce
integration errors are exponentially harder to fix in an unproven system.

Each new domain in Phase 3 follows the same E1-E8 implementation path
with the same pitfalls documented in phase-1.md. There are no shortcuts.
