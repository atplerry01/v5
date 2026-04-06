# TITLE
Remove All In-Memory Execution — Phase 1 Hardening

# CONTEXT
System was running on in-memory implementations for Phase 1 sandbox. This hardening prompt eliminates all in-memory code and enforces real infrastructure (Postgres, Kafka, Redis, OPA).

# CLASSIFICATION
- Classification: operational-system
- Context: infrastructure
- Domain: hardening

# OBJECTIVE
Eliminate ALL in-memory implementations and enforce real infrastructure adapters only. No fallbacks, no dual mode.

# CONSTRAINTS
- Zero InMemory references in codebase
- No hardcoded localhost fallbacks
- Projections triggered by Kafka consumers ONLY
- Infrastructure unavailability = throw (no fallback)
- All adapters backed by real infrastructure

# EXECUTION STEPS
1. Deleted InMemoryEventStore, InMemoryChainAnchor, InMemoryRedisClient, InMemoryOutbox, InMemoryIdempotencyStore, AllowAllPolicyEvaluator
2. Replaced with PostgresEventStoreAdapter, WhyceChainPostgresAdapter, StackExchangeRedisClient, PostgresOutboxAdapter, PostgresIdempotencyStoreAdapter, OpaPolicyEvaluator
3. Removed direct ProjectionDispatcher from EventFabric (projections via Kafka only)
4. Added KafkaOutboxPublisher as hosted background service
5. Removed all hardcoded localhost fallbacks — config throws if missing
6. Moved IIdempotencyStore to shared contracts (audit fix)
7. Created new adapter files: OpaPolicyEvaluator.cs, PostgresIdempotencyStoreAdapter.cs

# OUTPUT FORMAT
Change report with file-level actions.

# VALIDATION CRITERIA
- 0 InMemory references
- 0 compilation errors
- Projection flow: Outbox -> Kafka -> Consumer -> Projection
- Infrastructure required (throws if unavailable)
