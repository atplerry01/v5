using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Platform.Host.Bootstrap;

/// <summary>
/// Fail-fast bootstrap validator for HSID v2.1 infrastructure. Calls
/// <see cref="ISequenceStore.HealthCheckAsync"/> exactly once at host start
/// and throws if the sequence backend is not ready (missing
/// <c>hsid_sequences</c> table, unreachable database, drifted schema).
///
/// Guard reference: deterministic-id.guard.md G19, G20.
/// Audit reference: deterministic-id.audit.md A16, A17.
/// </summary>
public sealed class HsidInfrastructureValidator
{
    private readonly ISequenceStore _sequenceStore;

    public HsidInfrastructureValidator(ISequenceStore sequenceStore)
    {
        _sequenceStore = sequenceStore;
    }

    public async Task ValidateAsync()
    {
        var ok = await _sequenceStore.HealthCheckAsync();
        if (!ok)
        {
            throw new InvalidOperationException(
                "HSID FATAL: sequence store not ready. " +
                "Missing or drifted 'hsid_sequences' table. " +
                "Apply infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql " +
                "before starting the host.");
        }
    }
}
