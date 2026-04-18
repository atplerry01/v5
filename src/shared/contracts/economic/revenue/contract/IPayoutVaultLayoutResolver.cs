namespace Whycespace.Shared.Contracts.Economic.Revenue.Contract;

/// <summary>
/// Resolves vault routing required to execute a payout for a confirmed
/// distribution (Phase 3 T3.3). Returns the SPV's source vault id and the
/// per-participant target vault ids. Read-side port — resolution is against
/// the vault projection / capital allocation read model, never a write-side
/// aggregate load.
/// </summary>
public interface IPayoutVaultLayoutResolver
{
    Task<PayoutVaultLayout> ResolveAsync(
        string spvId,
        IReadOnlyCollection<string> participantIds,
        CancellationToken cancellationToken = default);
}

public sealed record PayoutVaultLayout(
    Guid SpvVaultId,
    IReadOnlyDictionary<string, Guid> ParticipantVaultIds);
