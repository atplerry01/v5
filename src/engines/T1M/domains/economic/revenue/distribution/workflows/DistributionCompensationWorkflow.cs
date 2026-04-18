// Phase 7 T7.3 — DistributionCompensationWorkflow.
// Steps are registered by the platform composition layer (pipeline batch B1).
// Canonical orchestration:
//   RequestDistributionCompensationStep -> MarkDistributionCompensatedStep
// Trigger: emitted by PayoutCompensationWorkflow after a compensating
// ledger journal has been posted successfully.
