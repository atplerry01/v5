# TITLE
Phase 2.8 Trust-System Platform API — Registry, Profile, Consent, Session Controllers

# CONTEXT
Phase 2.8 WhyceID implementation. Prior batches completed:
- Domain + engine layers (D2 activation, 143 tests)
- Event schema layer (17 BCs, DomainSchemaCatalog, TrustSystemCompositionRoot)
- Projections (2.8.17): Profile, Consent, Session read models + handlers + Kafka workers

# OBJECTIVE
Implement 2.8.14 — Trust-system Platform API for 4 BCs with full command dispatch and read-model queries.

BCs covered: Registry (identity lifecycle), Profile (identity profile), Consent (consent management), Session (access session)

# CONSTRAINTS
- Follows CapitalControllerBase / AccountController pattern from economic system
- Commands implement IHasAggregateId from Whycespace.Shared.Contracts.Runtime
- T2E handlers: creation uses static factory; lifecycle transitions use context.LoadAggregateAsync
- Routes: api/trust/identity/{bc}, api/trust/access/{bc}
- DomainRoute tuples: ("trust", "identity", "registry"), ("trust", "access", "session"), etc.
- Deterministic IDs via IIdGenerator.Generate(seed string)
- Session IDs include caller-provided SessionNonce for uniqueness
- ApiExplorerSettings GroupName: trust.{context}.{domain}
- No Guid.NewGuid, no system time (use IClock)

# EXECUTION STEPS
1. Shared contract commands: RegistryCommands, ProfileCommands, ConsentCommands, SessionCommands
2. T2E handlers: 4 registry + 3 profile + 3 consent + 3 session = 13 handlers
3. Application modules: RegistryApplicationModule, ProfileApplicationModule, ConsentApplicationModule, SessionApplicationModule
4. TrustControllerBase (shared base with Dispatch + LoadReadModel helpers)
5. 4 controllers: RegistryController, ProfileController, ConsentController, SessionController
6. Update TrustSystemCompositionRoot: wire AddXxxApplication() + RegisterEngines

# OUTPUT FORMAT
- 4 shared contract command files
- 13 T2E handler files
- 4 application module files
- TrustControllerBase + 4 controllers = 5 files
- 1 updated TrustSystemCompositionRoot

# VALIDATION CRITERIA
- dotnet build exits 0, no new errors
- Test suite stable (no regressions from prior 1166 passing)
