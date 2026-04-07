# v5

This is the v5 project repository.


| Area   | Before              | After                      |
| ------ | ------------------- | -------------------------- |
| Time   | Non-deterministic ❌ | Replay-safe ✅              |
| IDs    | Random ❌            | Deterministic ✅            |
| Chain  | Weak ❌              | Cryptographically stable ✅ |
| Outbox | Duplicate risk ❌    | Exactly-once semantics ✅   |

E1 → Domain
E2 → Contracts
E3 → Persistence
E4 → Determinism
E5 → Engine
E6 → Runtime
E7 → Workflow
E8 → Projection
E9 → Policy
E10 → Guard
E11 → Chain
E12 → Full enforcement

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

## TODO
- Add failure + compensation flow to Todo workflow
- fix domain entity scattered in code base
- program.cs file too large lets break it down
- upgrade todo to trello/kaban style to accomodate lifecycle/workflow

- multistep workflow calling different domain services using todo/incident as case study

- upgrade runtime
- 1m RPS upgrade

- clean domain model
- fix project dependency
- connection string in code

- deteministic id
- update the new generated guard rules
- runtime workflow and workflow-state fix





We are not invetorment platform, but platform that help you scale your investment portforlio
True economic activities, not extractive activities
Team work
African can continue in its complaint tactics
Building ecosystem

Real world economic structure /not short time, not 
the poer of upbringing and associations


- get a skill
- 