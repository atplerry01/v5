using Whycespace.Engines.T0U.WhyceChain.Command;
using Whycespace.Engines.T0U.WhyceChain.Hashing;
using Whycespace.Engines.T0U.WhyceChain.Integrity;
using Whycespace.Engines.T0U.WhyceChain.Result;
using Whycespace.Engines.T0U.WhyceChain.Sequencing;

namespace Whycespace.Engines.T0U.WhyceChain.Engine;

/// <summary>
/// WhyceChain — Constitutional Chain Engine (T0U).
/// Non-bypassable. Every persisted event MUST be anchored to the chain.
///
/// STATELESS: All chain state (last block hash, sequence) is provided via the command.
/// The runtime layer owns chain state; this engine is a pure processor.
///
/// Hash Rule: SHA256(previousHash + payloadHash + sequence)
/// NO timestamps in hash computation.
///
/// Capabilities:
/// - Anchor: Anchor events to the chain with deterministic hashing
/// - ValidatePreviousHash: Verify chain link integrity
/// - GenerateBlockHash: Compute deterministic block hash
/// - EnforceSequence: Validate monotonic sequence ordering
/// </summary>
public sealed class WhyceChainEngine
{
    /// <summary>
    /// Anchors events to the chain.
    /// Validates sequence, computes hashes, verifies chain linkage.
    /// Stateless: all inputs provided via command.
    /// </summary>
    public Task<ChainAnchorResult> Anchor(AnchorEventsCommand command)
    {
        if (command.Events.Count == 0)
        {
            return Task.FromResult(new ChainAnchorResult(
                IsAnchored: false,
                BlockHash: string.Empty,
                EventHash: string.Empty,
                PreviousBlockHash: string.Empty,
                Sequence: command.Sequence,
                FailureReason: "Cannot anchor empty event list to WhyceChain."));
        }

        // Step 1: Validate previous hash is present (structural validity)
        // Chain linkage validation happens at the runtime layer which owns chain state.
        // The engine validates that the provided hash is non-empty — it cannot self-compare
        // because the engine is stateless and does not store the chain tip.
        if (string.IsNullOrWhiteSpace(command.PreviousBlockHash))
        {
            return Task.FromResult(new ChainAnchorResult(
                IsAnchored: false,
                BlockHash: string.Empty,
                EventHash: string.Empty,
                PreviousBlockHash: string.Empty,
                Sequence: command.Sequence,
                FailureReason: "PreviousBlockHash is required. Chain cannot anchor without link to previous block."));
        }

        // Step 2: Enforce sequence (stateless — last known sequence passed in command)
        var (seqValid, seqViolation) = EnforceSequence(command.Sequence, command.LastKnownSequence);
        if (!seqValid)
        {
            return Task.FromResult(new ChainAnchorResult(
                IsAnchored: false,
                BlockHash: string.Empty,
                EventHash: string.Empty,
                PreviousBlockHash: command.PreviousBlockHash,
                Sequence: command.Sequence,
                FailureReason: seqViolation));
        }

        // Step 3: Compute event payload hash
        var payloadHash = ChainHasher.ComputePayloadHash(command.Events);

        // Step 4: Generate block hash: SHA256(previousHash + payloadHash + sequence)
        var blockHash = GenerateBlockHash(
            command.PreviousBlockHash, payloadHash, command.Sequence);

        return Task.FromResult(new ChainAnchorResult(
            IsAnchored: true,
            BlockHash: blockHash,
            EventHash: payloadHash,
            PreviousBlockHash: command.PreviousBlockHash,
            Sequence: command.Sequence,
            FailureReason: null));
    }

    /// <summary>
    /// Validates that the provided previous hash matches the expected chain tip.
    /// </summary>
    public static (bool IsValid, string? Violation) ValidatePreviousHash(
        string expectedHash, string providedHash)
    {
        return IntegrityValidator.ValidateBlockLink(expectedHash, providedHash);
    }

    /// <summary>
    /// Generates a deterministic block hash.
    /// Hash = SHA256(previousHash + payloadHash + sequence)
    /// </summary>
    public static string GenerateBlockHash(string previousHash, string payloadHash, long sequence)
    {
        return ChainHasher.ComputeBlockHash(previousHash, payloadHash, sequence);
    }

    /// <summary>
    /// Enforces monotonic sequence ordering. Stateless — requires last known sequence.
    /// </summary>
    public static (bool IsValid, string? Violation) EnforceSequence(long sequence, long lastKnownSequence)
    {
        return SequenceEnforcer.ValidateSequence(sequence, lastKnownSequence);
    }
}
