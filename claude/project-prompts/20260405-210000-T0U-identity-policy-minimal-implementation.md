# TITLE
T0U Minimal Implementation — WhyceID + WhycePolicy Engines

# CONTEXT
Classification: T0U / identity / policy
Layer: engines (T0U tier) + runtime (middleware integration)
Prior state: T0U folder empty (.gitkeep only), PolicyMiddleware delegated to IPolicyEvaluator

# OBJECTIVE
Implement minimal deterministic identity resolution (WhyceIdEngine) and policy evaluation (WhycePolicyEngine) as T0U utility engines. Integrate into runtime middleware pipeline so identity is resolved and policy is enforced before any T2E domain execution.

# CONSTRAINTS
- T0U engines MUST be stateless
- T0U engines MUST NOT persist
- T0U engines MUST NOT call external services
- T0U engines MUST NOT import domain
- T0U executes BEFORE T1M and T2E
- Policy MUST be evaluated AFTER identity resolution
- Deterministic SHA256 hashing for identity tokens and policy decisions
- No Guid.NewGuid(), no DateTime.Now

# EXECUTION STEPS
1. Batch 1: Created WhyceIdEngine in src/engines/T0U/whyceid/ (WhyceIdCommand, WhyceIdentity, WhyceIdResult, WhyceIdEngine)
2. Batch 2: Created WhycePolicyEngine in src/engines/T0U/whycepolicy/ (WhycePolicyCommand, PolicyDecision, WhycePolicyEngine)
3. Batch 3: Rewired PolicyMiddleware to call WhyceIdEngine → inject identity → WhycePolicyEngine → inject decision
4. Batch 4: Extended CommandContext with IdentityId, Roles, TrustScore, PolicyDecisionAllowed, PolicyDecisionHash
5. Batch 5: Added CommandId to CommandContext; updated SystemIntentDispatcher to generate deterministic CommandId

# OUTPUT FORMAT
New files created:
- src/engines/T0U/whyceid/WhyceIdCommand.cs
- src/engines/T0U/whyceid/WhyceIdentity.cs
- src/engines/T0U/whyceid/WhyceIdResult.cs
- src/engines/T0U/whyceid/WhyceIdEngine.cs
- src/engines/T0U/whycepolicy/WhycePolicyCommand.cs
- src/engines/T0U/whycepolicy/PolicyDecision.cs
- src/engines/T0U/whycepolicy/WhycePolicyEngine.cs

Modified files:
- src/shared/contracts/runtime/CommandContext.cs
- src/runtime/pipeline/PolicyMiddleware.cs
- src/runtime/pipeline/SystemIntentDispatcher.cs
- src/platform/host/Program.cs

# VALIDATION CRITERIA
- Build succeeds with 0 errors, 0 warnings
- WhyceID resolves identity deterministically (SHA256 token hashing)
- WhycePolicy returns allow/deny with deterministic DecisionHash
- Middleware injects identity + policy into CommandContext
- Policy blocks execution when denied
- No external dependencies in T0U engines
- No domain imports in T0U engines
