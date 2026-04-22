# TITLE
Phase 2.8.2 Trust-System Authentication Engine — Credential + Verification BCs

# CONTEXT
Phase 2.8 WhyceID implementation. Prior batches completed:
- Domain + engine layers (D2 activation, 143 tests)
- Event schema layer (17 BCs, DomainSchemaCatalog, TrustSystemCompositionRoot)
- Projections (2.8.17): Profile, Consent, Session read models + handlers + Kafka workers
- Platform API (2.8.14): Registry, Profile, Consent, Session controllers + T2E handlers
- Registration Onboarding Workflow (2.8.1a): T1M 3-step workflow + trigger handler + worker

# OBJECTIVE
Implement 2.8.2 — Authentication Engine for 2 BCs with full vertical slice:
- Credential BC: issue, activate, revoke credential lifecycle
- Verification BC: initiate, pass, fail verification lifecycle

# CONSTRAINTS
- Follows existing Trust-system patterns (RegistryController, ProfileProjectionHandler, etc.)
- Commands implement IHasAggregateId from Whycespace.Shared.Contracts.Runtime
- T2E creation handlers: static factory; lifecycle handlers: context.LoadAggregateAsync
- Routes: api/trust/identity/credential, api/trust/identity/verification
- DomainRoute tuples: ("trust", "identity", "credential"), ("trust", "identity", "verification")
- Deterministic IDs via IIdGenerator.Generate(seed string)
- ApiExplorerSettings GroupName: trust.identity.credential / trust.identity.verification
- No Guid.NewGuid, no system time (use IClock)

# EXECUTION STEPS
1. Shared contract commands: CredentialCommands, VerificationCommands
2. T2E handlers: 3 credential + 3 verification = 6 handlers
3. Application modules: CredentialApplicationModule, VerificationApplicationModule
4. Read models: CredentialReadModel, VerificationReadModel
5. Reducers: CredentialProjectionReducer, VerificationProjectionReducer
6. Projection handlers: CredentialProjectionHandler, VerificationProjectionHandler
7. Update TrustProjectionModule: add credential + verification stores, handlers, workers
8. Controllers: CredentialController, VerificationController
9. Update TrustSystemCompositionRoot: AddCredentialApplication, AddVerificationApplication

# OUTPUT FORMAT
- 2 shared contract command files
- 6 T2E handler files
- 2 application module files
- 2 read model files
- 2 reducer files
- 2 projection handler files
- 1 updated TrustProjectionModule
- 2 controller files
- 1 updated TrustSystemCompositionRoot

# VALIDATION CRITERIA
- dotnet build exits 0, no new errors
- Test suite stable (no regressions from prior 1167 passing)
