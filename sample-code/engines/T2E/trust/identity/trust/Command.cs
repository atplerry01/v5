namespace Whycespace.Engines.T2E.Trust.Identity.Trust;

public record TrustCommand(string Action, string EntityId, object Payload);
public sealed record InitializeTrustCommand(string IdentityId) : TrustCommand("Initialize", IdentityId, null!);
public sealed record RecordTrustFactorCommand(string TrustProfileId, string IdentityId, string Factor, decimal Weight) : TrustCommand("RecordFactor", TrustProfileId, null!);
public sealed record FreezeTrustCommand(string TrustProfileId, string Reason) : TrustCommand("Freeze", TrustProfileId, null!);
public sealed record UnfreezeTrustCommand(string TrustProfileId) : TrustCommand("Unfreeze", TrustProfileId, null!);
