CLASSIFICATION: meta / prompt-reconciliation
SOURCE: 20260407-220000-runtime-workflow-kafka-projection-replay.md
SEVERITY: S2 — MEDIUM (process)

DESCRIPTION:
Pasted "WBSM v3.5" prompt proposed inventing Kafka/projection abstractions
(`IKafkaConsumer`, `KafkaMessage`, `IProjectionDispatcher`, `ProjectionDispatcher`,
`WorkflowStateProjectionHandler`, `WorkflowStateReadModel`,
`IWorkflowStateProjectionStore`, `WorkflowAggregate`) that already exist in the
codebase under canonical names (`GenericKafkaProjectionConsumerWorker`,
`IProjectionDispatcher` in `src/runtime/projection/`, `ProjectionRegistry`,
`WorkflowExecutionProjectionHandler`, `WorkflowExecutionReadModel`,
`IWorkflowExecutionProjectionStore`, `WorkflowExecutionAggregate`).

Prompt also referenced wrong API (`IEventStore.LoadAsync` — actual is
`LoadEventsAsync`), wrong path casing (`src/engines/t1m/` — actual `T1M`),
and wrong topic format (`whyce.workflow.execution.events` — canonical form
per `TopicNameResolver` is `whyce.orchestration-system.workflow.events`).

Literal execution would have produced parallel duplicate types, broken the
build, and violated $5 (anti-drift) by introducing new naming.

PROPOSED_RULE / REMEDIATION:
Add a pre-execution reconciliation step to canonical execution flow ($1):
before code emission, verify every type/interface/method/path/topic named
in the prompt against the current codebase. Diverge from prompt literal
text whenever the codebase has a canonical equivalent; record the
divergence in the prompt's RECONCILIATION section.

Candidate guard (proposed):
  PROMPT-RECONCILE-01 — Pasted prompts must be reconciled against existing
  surface area. Prompt-named types that differ from canonical names must
  not be created; the canonical name must be used and the divergence
  recorded inline in the project-prompt file.

TRACKED BY: (not yet promoted)

SEVERITY JUSTIFICATION: S2 because literal execution would have caused
build breakage and architectural drift, but the failure mode is detectable
in code review and does not bypass policy or audit.
