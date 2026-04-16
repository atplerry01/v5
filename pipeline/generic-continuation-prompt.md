Use the standard Whycespace domain implementation template.

use @/pipeline/execution_context.md for the input below

Implement this batch as:
CLASSIFICATION: <classification>
CONTEXT: <context>
DOMAIN GROUP: <domain-group>
DOMAINS:
- <domain-1>
- <domain-2>
- <domain-3>

Requirements:
- batch must cover the full domain group inside the context
- use canonical hierarchy: CLASSIFICATION → CONTEXT → DOMAIN GROUP → DOMAIN
- implement in stages E1 → EX
- minimum target: S4 domain standard
- deterministic only
- no infrastructure leakage in domain layer
- include invariants, specifications, errors, events, commands, queries, projection needs, API needs, policy needs, E2E validation notes
- only add workflow if justified
- return: batch summary, scope confirmation, stage-by-stage implementation plan, file tree, change report, validation checklist, risks/gaps

Begin with Stage E1 for all domains in the domain group, then continue through later stages as requested.

###