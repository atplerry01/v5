// Phase 7 T7.3 — PayoutCompensationWorkflow.
// Steps are registered by the platform composition layer (pipeline batch B2).
// Canonical orchestration:
//   RequestPayoutCompensationStep
//     -> PostCompensatingLedgerJournalStep  (T7.4 append-only reversal)
//     -> MarkPayoutCompensatedStep
// On success a DistributionCompensationIntent is emitted to chain the
// upstream DistributionCompensationWorkflow.
