namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.C.1 — C# mirror of
/// <c>infrastructure/observability/certification/replay-equivalence.yml</c>.
/// Validators consume this record list so they don't depend on a YAML
/// parser NuGet. The YAML remains the human-readable source of truth;
/// <c>ReplayInvariantManifestTests</c> pins the two in sync.
/// </summary>
public static class CanonicalReplayInvariants
{
    public sealed record ReplayInvariant(
        string Id,
        string InvariantFamily,
        IReadOnlyList<string> CanonicalPrimitives,
        string? ProofTest,
        string Status);

    public const string CertifiedStatus = "certified";
    public const string UnprovenStatus = "unproven";

    public static readonly IReadOnlyList<ReplayInvariant> All = new ReplayInvariant[]
    {
        new(
            Id: "command-reexecution-deterministic-end-to-end",
            InvariantFamily: "type-a",
            CanonicalPrimitives: new[] { "ExecutionHash", "DeterministicIdGenerator", "ChainHasher", "TestClock" },
            ProofTest: "tests/e2e/replay/DeterministicReplayTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "aggregate-stream-replay-byte-equivalent",
            InvariantFamily: "type-b",
            CanonicalPrimitives: new[] { "OutboundEffectAggregate", "EventDeserializer" },
            ProofTest: "tests/integration/integration-system/outbound-effect/OutboundEffectAggregateReplayEquivalenceTests.cs",
            Status: CertifiedStatus),

        new(
            Id: "event-envelope-lossless-roundtrip",
            InvariantFamily: "round-trip",
            CanonicalPrimitives: new[] { "AggregateReplayHarness", "EventDeserializer", "EventSchemaRegistry" },
            ProofTest: "tests/integration/setup/AggregateReplayHarness.cs",
            Status: CertifiedStatus),

        new(
            Id: "deterministic-id-same-seed-same-guid",
            InvariantFamily: "primitive",
            CanonicalPrimitives: new[] { "DeterministicIdGenerator", "TestClock" },
            ProofTest: "tests/integration/economic-system/phase2-validation/Phase2DeterminismValidationTests.cs",
            Status: CertifiedStatus),

        new(
            Id: "workflow-resume-replay-deterministic",
            InvariantFamily: "workflow-replay",
            CanonicalPrimitives: new[] { "WorkflowExecutionReplayService", "T1MWorkflowEngine" },
            ProofTest: "tests/integration/orchestration/workflow/execution/WorkflowReplayResumeTests.cs",
            Status: CertifiedStatus),

        new(
            Id: "workflow-approval-replay-deterministic",
            InvariantFamily: "workflow-replay",
            CanonicalPrimitives: new[] { "IWorkflowExecutionReplayService", "ApprovalDecisionPayload" },
            ProofTest: "tests/integration/orchestration/workflow/execution/WorkflowApprovalReplayServiceTests.cs",
            Status: CertifiedStatus),

        new(
            Id: "command-context-replay-reset-preserves-invariants",
            InvariantFamily: "replay-seam",
            CanonicalPrimitives: new[] { "CommandContext" },
            ProofTest: "tests/unit/runtime/CommandContextReplayResetTests.cs",
            Status: CertifiedStatus),

        new(
            Id: "phase7-cross-subsystem-replay-determinism",
            InvariantFamily: "projection-replay",
            CanonicalPrimitives: new[] { "ProjectionDispatcher", "EventSchemaRegistry" },
            ProofTest: "tests/unit/phase7/Phase7ReplayDeterminismTests.cs",
            Status: CertifiedStatus),
    };

    public static readonly IReadOnlyList<ReplayInvariant> UnprovenInvariants = new ReplayInvariant[]
    {
        new(
            Id: "projection-state-byte-equivalence-after-full-rebuild",
            InvariantFamily: "projection-rebuild",
            CanonicalPrimitives: new[] { "ProjectionRebuilder", "EventReplayService", "PostgresProjectionStore" },
            ProofTest: null,
            Status: UnprovenStatus),

        new(
            Id: "cross-instance-replay-equivalence",
            InvariantFamily: "multi-instance",
            CanonicalPrimitives: new[] { "GenericKafkaProjectionConsumerWorker", "ProjectionRebuilder" },
            ProofTest: null,
            Status: UnprovenStatus),

        new(
            Id: "chain-anchor-ledger-replay-equivalence",
            InvariantFamily: "chain-replay",
            CanonicalPrimitives: new[] { "ChainAnchorService", "ChainHasher" },
            ProofTest: null,
            Status: UnprovenStatus),
    };
}
