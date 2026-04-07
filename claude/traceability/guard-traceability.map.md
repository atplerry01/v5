# guard-traceability.map.md

## TRACEABILITY MAP — 2026-04-07

---

### 1. POLICY ENFORCEMENT (T0U)

RULE: POLICY-ENFORCEMENT-01
AUDIT: CHECK-R-POLICY-FIRST-01
ENFORCEMENT POINT:
- RuntimeControlPlane.ExecuteAsync(...)
- PolicyMiddleware.InvokeAsync(...)

MECHANISM:
- Policy evaluated BEFORE aggregate load
- Deny short-circuits pipeline

EVIDENCE:
- PolicyDecision (Allow/Deny)
- DecisionHash
- PolicyVersion
- Chain Anchor Record (WhyceChain)

---

RULE: POLICY-CHAIN-01
AUDIT: CHECK-POLICY-CHAIN-01
ENFORCEMENT POINT:
- PolicyDecisionAnchorService.AnchorAsync(...)

MECHANISM:
- DecisionHash written to WhyceChain BEFORE Kafka publish

EVIDENCE:
- BlockId
- PreviousHash
- PayloadHash
- DecisionHash

---

RULE: POLICY-DETERMINISM-01
AUDIT: CHECK-DET-POLICY-01
ENFORCEMENT POINT:
- PolicyEngineInvoker.InvokeAsync(...)
- DecisionHashBuilder.Build(...)

MECHANISM:
- Hash excludes timestamps/random inputs

EVIDENCE:
- Stable DecisionHash across replay

---

### 2. RUNTIME CONTROL PLANE

RULE: R-CANONICAL-PIPELINE-01
AUDIT: CHECK-R-ORDER-01
ENFORCEMENT POINT:
- RuntimeControlPlane.ExecuteAsync(...)

MECHANISM:
Execution order MUST be:
1. Validation
2. Identity
3. Policy
4. Idempotency
5. Execution
6. Persistence
7. Chain
8. Kafka

EVIDENCE:
- Ordered middleware logs
- Execution trace spans

---

RULE: R-UOW-01
AUDIT: CHECK-R-UOW-01
ENFORCEMENT POINT:
- EventStore.AppendAsync(...)
- UnitOfWork.CommitAsync(...)

MECHANISM:
- All events persisted atomically before publish

EVIDENCE:
- EventStore version increments
- Transaction commit logs

---

RULE: R-CQRS-01
AUDIT: CHECK-R-CQRS-01
ENFORCEMENT POINT:
- RuntimeCommandDispatcher
- QueryHandlers (separate path)

MECHANISM:
- Commands never read projections
- Queries never mutate domain

EVIDENCE:
- No projection access in command path (static analysis)
- Separate query pipeline logs

---

### 3. ENGINE PURITY (T2E)

RULE: ENG-PURITY-01
AUDIT: CHECK-ENG-PURITY-01
ENFORCEMENT POINT:
- All engines under src/engines/

MECHANISM:
- No infra/runtime imports
- No persistence calls
- Only EngineContext.EmitEvents(...)

EVIDENCE:
- Static dependency scan
- No DbContext/HTTP/Kafka usage in engines

---

RULE: ENG-DOMAIN-ALIGN-01
AUDIT: CHECK-ENG-DOMAIN-ALIGN-01
ENFORCEMENT POINT:
- Engine ↔ Aggregate mapping

MECHANISM:
- One engine = one domain aggregate

EVIDENCE:
- Namespace alignment
- No cross-domain references

---

### 4. WORKFLOW (T1M)

RULE: WF-PLACEMENT-01
AUDIT: CHECK-WF-PLACEMENT-01
ENFORCEMENT POINT:
- engines/T1M/*
- systems/midstream (declarative only)

MECHANISM:
- Workflow logic exists ONLY in T1M
- Systems define, do not execute

EVIDENCE:
- No execution code in systems/*
- Workflow handlers in T1M only

---

RULE: WF-PIPELINE-01
AUDIT: CHECK-R-WORKFLOW-PIPE-01
ENFORCEMENT POINT:
- WorkflowStartCommand → RuntimeControlPlane

MECHANISM:
- Workflow goes through full middleware

EVIDENCE:
- PolicyDecision + ExecutionHash present for workflow steps

---

### 5. EVENT SOURCING + ECONOMIC

RULE: ECON-ES-01
AUDIT: CHECK-ECON-ES-01
ENFORCEMENT POINT:
- Domain aggregates

MECHANISM:
- State changes ONLY via events

EVIDENCE:
- RaiseDomainEvent(...) usage
- No direct state mutation outside handlers

---

RULE: ECON-LEDGER-01
AUDIT: CHECK-ECON-LEDGER-01
ENFORCEMENT POINT:
- CapitalAccountAggregate / Ledger aggregates

MECHANISM:
- Invariants checked BEFORE event emission

EVIDENCE:
- Balanced ledger assertions
- Domain validation logs

---

### 6. PROJECTIONS (READ MODEL)

RULE: PROJ-READ-ONLY-01
AUDIT: CHECK-PROJ-READ-ONLY-01
ENFORCEMENT POINT:
- src/projections/*

MECHANISM:
- No domain mutation allowed

EVIDENCE:
- No domain aggregate references
- Write only to read model storage

---

### 7. KAFKA

RULE: K-TOPIC-CANONICAL-01
AUDIT: CHECK-K-TOPIC-CANONICAL-01
ENFORCEMENT POINT:
- TopicNameResolver.Resolve(...)

MECHANISM:
- whyce.{cluster}.{context}.{event}
- past tense enforced

EVIDENCE:
- Topic validation logs

---

RULE: K-DETERMINISTIC-01
AUDIT: CHECK-K-DETERMINISTIC-01
ENFORCEMENT POINT:
- DeterministicPartitionResolver.Resolve(...)

MECHANISM:
- FNV-1a hash or equivalent

EVIDENCE:
- Same key → same partition

---

### 8. DETERMINISM (GLOBAL)

RULE: D-DET-01
AUDIT: CHECK-DET-GLOBAL-01
ENFORCEMENT POINT:
- All layers (domain/engine/runtime/platform)

MECHANISM:
- No Guid.NewGuid()
- No DateTime.UtcNow
- All via injected providers

EVIDENCE:
- Static scan results
- Replay consistency tests

---

### 9. IDENTITY

RULE: ID-POLICY-01
AUDIT: CHECK-ID-POLICY-01
ENFORCEMENT POINT:
- IdentityMiddleware → PolicyMiddleware

MECHANISM:
- Identity resolved before policy

EVIDENCE:
- Identity context present in policy input

---

### 10. OBSERVABILITY + REPLAY

RULE: OBS-TRACE-01
AUDIT: CHECK-OBS-TRACE-01
ENFORCEMENT POINT:
- Runtime logging / tracing layer

MECHANISM:
- All executions emit trace metadata

EVIDENCE:
- EventId, ExecutionHash, DecisionHash logs

---

RULE: OBS-REPLAY-01
AUDIT: CHECK-OBS-REPLAY-01
ENFORCEMENT POINT:
- Replay engine / test harness

MECHANISM:
- Replay produces identical results

EVIDENCE:
- Hash comparison (original vs replay)
