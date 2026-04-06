namespace Whycespace.Engines.T0U.WhyceChain;

public abstract record ChainCommand;

public sealed record RecordEvidenceCommand(string EvidenceId, string Payload, string Source) : ChainCommand;

public sealed record AnchorMerkleCommand(string RootHash, IReadOnlyList<string> Leaves) : ChainCommand;

public sealed record VerifyBlockCommand(string BlockId) : ChainCommand;

public sealed record VerifyChainCommand : ChainCommand;

public sealed record GetAuditTrailCommand(string AggregateId) : ChainCommand;
