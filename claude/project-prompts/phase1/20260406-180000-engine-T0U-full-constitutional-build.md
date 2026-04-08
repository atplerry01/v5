# TITLE
Full T0U Implementation — WhyceId + WhycePolicy + WhyceChain Constitutional Engine Build

# CONTEXT
T0U engines (identity, policy, chain) existed as stub-level implementations with flat file structures.
This prompt upgrades all three to fully operational constitutional engines with proper subdirectory organization,
full engine capabilities, shared contracts, and runtime integration.

# CLASSIFICATION
engine / T0U / constitutional

# OBJECTIVE
Upgrade T0U from stub-level to fully operational constitutional layer integrated with runtime:
- WhyceId: 6 capabilities (AuthenticateIdentity, ResolveIdentity, ValidateSession, EvaluateTrustScore, VerifyIdentity, ResolveRolesAndAttributes)
- WhycePolicy: Full evaluation with safeguards, registry, simulation, conflict detection
- WhyceChain: Anchor, ValidatePreviousHash, GenerateBlockHash, EnforceSequence
- Runtime: PolicyMiddleware, EventFabric ChainAnchorService, Host DI registration
- Contracts: identity/, policy/, chain/ under shared/contracts/

# CONSTRAINTS
- No Guid.NewGuid(), no DateTime.UtcNow — deterministic only
- SHA256 hashing exclusively
- Chain hash: SHA256(previousHash + payloadHash + sequence) — no timestamps
- Non-bypassable: No execution without WhycePolicy, no event without WhyceChain, no request without WhyceId
- Engines are stateless processors
- All records sealed and immutable

# EXECUTION STEPS
1. Pre-execution guard loading (all 14 guard files)
2. Build WhyceId engine (11 subdirectories, 24 files)
3. Build WhycePolicy engine (10 subdirectories, 11 files)
4. Build WhyceChain engine (8 subdirectories, 8 files)
5. Create shared contracts (identity, policy, chain — 7 files)
6. Update runtime integration (PolicyMiddleware, ChainAnchorService, EventFabric ChainAnchorService)
7. Update host DI registration
8. Full solution build verification
9. Determinism verification (zero Guid.NewGuid, zero DateTime.UtcNow in T0U)
10. Non-bypassable enforcement verification

# OUTPUT FORMAT
Change report, new structure, engine capabilities, integration points, compliance score

# VALIDATION CRITERIA
- Full solution builds with 0 warnings, 0 errors
- Zero determinism violations in T0U engines
- Three constitutional guarantees enforced in runtime
- All engine capabilities implemented per specification
