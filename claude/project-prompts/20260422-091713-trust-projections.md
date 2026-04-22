# TITLE
Phase 2.8 Trust-System Projections — Profile, Consent, Session Read Models

# CONTEXT
Phase 2.8 WhyceID implementation. Prior batches completed:
- Domain + engine layers (17 trust-system aggregates, 143 tests, D2 activation)
- WhyceId Engine T0U (BuildAuthorizationContext wired in PolicyMiddleware)
- Event schema layer (17 schema modules, DomainSchemaCatalog, TrustSystemCompositionRoot, BootstrapModuleCatalog)

# OBJECTIVE
Implement 2.8.17 — Trust-system projections/read models for the 3 highest-value BCs:
1. Identity/Profile — ProfileReadModel tracking created/activated/deactivated lifecycle
2. Identity/Consent — ConsentReadModel tracking granted/revoked/expired lifecycle
3. Access/Session — SessionReadModel tracking opened/expired/terminated lifecycle

# CONSTRAINTS
- Follows canonical EconomicProjectionModule pattern: ProjectionStoreFactory, handler + reducer split, GenericKafkaProjectionConsumerWorker per topic
- Reducer uses `state with { ... }` — immutable record update
- Handler implements IEnvelopeProjectionHandler + IProjectionHandler<T> per event schema
- Kafka topics follow formula: whyce.trust.{context}.{domain}.events
- Consumer groups: whyce.projection.trust.{context}.{domain}
- Projection schema names: projection_trust_{context}_{domain}
- Layer purity: no domain dependencies in projections; shared contracts only
- No DateTimeOffset.UtcNow in Apply for creation events — use event's timestamp

# EXECUTION STEPS
1. Create read models in src/shared/contracts/trust/{context}/{domain}/
2. Create reducers in src/projections/trust/{context}/{domain}/reducer/
3. Create handlers in src/projections/trust/{context}/{domain}/
4. Create TrustProjectionModule in src/platform/host/composition/trust/projection/
5. Update TrustSystemCompositionRoot to call AddTrustProjection + TrustProjectionModule.RegisterProjections
6. Build and test

# OUTPUT FORMAT
- 9 new files (3 read models, 3 reducers, 3 handlers)
- 1 new TrustProjectionModule
- 1 updated TrustSystemCompositionRoot

# VALIDATION CRITERIA
- dotnet build exits 0, no new errors
- Test suite stable (no regressions from prior 1170 passing)
