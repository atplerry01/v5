# v5

This is the v5 project repository.


| Area   | Before              | After                      |
| ------ | ------------------- | -------------------------- |
| Time   | Non-deterministic ❌ | Replay-safe ✅              |
| IDs    | Random ❌            | Deterministic ✅            |
| Chain  | Weak ❌              | Cryptographically stable ✅ |
| Outbox | Duplicate risk ❌    | Exactly-once semantics ✅   |


Platform API
  → Systems.Downstream
  → Systems.Midstream (WSS/HEOS/WhyceAtlas when needed)
  → Runtime Control Plane
  → T0U (policy/guard/pre-flight)
  → T1M (workflow/orchestration execution, if workflow is involved)
  → T2E (domain execution)
  → Domain Aggregate / Domain Logic
  → Domain Events Emitted
  → Runtime persists / anchors / publishes / triggers projections
  → Systems
  → Platform API Response


Platform API receives and translates requests.
Systems select and compose the correct business/orchestration path.
Runtime governs execution through middleware, routing, persistence, publication, and projection triggering.
T0U evaluates policy and preconditions.
T1M coordinates workflow execution where required.
T2E performs domain-aligned command execution.
Domain aggregates enforce invariants and raise events.
Runtime persists, anchors, publishes, projects, and returns the final result upward to Systems and then Platform API.

Add failure + compensation flow to Todo workflow
