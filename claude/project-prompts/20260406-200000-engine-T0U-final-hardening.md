# TITLE
T0U Final Hardening — Pre-Validation Lock

# CONTEXT
T0U engines (identity, policy, chain) were built but needed hardening for determinism, replay safety, and non-bypassability.

# CLASSIFICATION
engine / T0U / hardening

# OBJECTIVE
Lock identity truth, policy authority, and chain integrity to be deterministic, replayable, and non-bypassable.

# CONSTRAINTS
- Trust score computed ONCE per request, never recomputed
- Policy version immutable during execution, replay uses SAME version
- Chain concurrency enforced, PreviousHash validated on every anchor
- Policy hard stop enforced at middleware AND control plane level
- ExecutionHash includes identity, policy, workflow, economic dimensions

# EXECUTION STEPS
1. Pre-execution guard loading (all 14 guard files)
2. Read all 22 T0U implementation files
3. Fix 1: Lock TrustScore/IdentityId/Roles as write-once on CommandContext
4. Fix 2: Add PolicyVersion to CommandContext; include in DecisionHash
5. Fix 3: Fix WhyceChainEngine PreviousBlockHash self-comparison bug
6. Fix 4: Add defense-in-depth policy hard-stop in RuntimeControlPlane
7. Fix 5: Include PolicyVersion in ExecutionHash for replay consistency
8. Fix 6: Remove duplicate WhyceChainAnchorService (dead code)
9. Full solution build verification
10. Determinism + statefulness verification
11. Post-execution audit sweep

# OUTPUT FORMAT
Change report, violations fixed, final score

# VALIDATION CRITERIA
- Full solution builds with 0 warnings, 0 errors
- Zero determinism violations in T0U engines
- Zero mutable state in T0U engines
- Write-once enforcement on CommandContext T0U fields
- Policy hard stop at both middleware and control plane levels
- PolicyVersion in DecisionHash and ExecutionHash
- No duplicate chain anchor services
