namespace Whycespace.Engines.T2E.Trust.Identity.IdentityGraph;

public record IdentityGraphCommand(string Action, string EntityId, object Payload);
public sealed record CreateIdentityGraphCommand(string PrimaryIdentityId) : IdentityGraphCommand("Create", PrimaryIdentityId, null!);
public sealed record LinkIdentitiesCommand(string GraphId, string SourceIdentityId, string TargetIdentityId, string LinkType, string Strength) : IdentityGraphCommand("Link", GraphId, null!);
public sealed record UnlinkIdentitiesCommand(string GraphId, string SourceIdentityId, string TargetIdentityId, string LinkType) : IdentityGraphCommand("Unlink", GraphId, null!);
public sealed record MergeGraphsCommand(string TargetGraphId, string SourceGraphId) : IdentityGraphCommand("Merge", TargetGraphId, null!);
public sealed record CloseGraphCommand(string GraphId) : IdentityGraphCommand("Close", GraphId, null!);
