namespace Whyce.Shared.Contracts.Chain;

/// <summary>
/// Contract for chain integrity validation.
/// Allows external systems to verify chain integrity.
/// </summary>
public interface IChainIntegrityValidator
{
    Task<ChainIntegrityResult> ValidateAsync(Guid correlationId);
    Task<ChainIntegrityResult> ValidateRangeAsync(long fromSequence, long toSequence);
}

public sealed record ChainIntegrityResult(
    bool IsValid,
    long BlocksValidated,
    string? FirstViolationAt,
    string? ViolationReason);
