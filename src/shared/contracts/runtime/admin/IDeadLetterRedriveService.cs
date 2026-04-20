namespace Whycespace.Shared.Contracts.Runtime.Admin;

/// <summary>
/// R4.B / R-ADMIN-REDRIVE-ELIGIBILITY-01 — sanctioned seam for operator-
/// initiated DLQ re-drive. Inspects the dead-letter entry, enforces strict
/// eligibility preconditions, re-publishes the raw payload to the original
/// source topic via <see cref="IDeadLetterRedrivePublisher"/>, and marks the
/// entry reprocessed on success.
///
/// <para>Every invocation — eligible or not — emits an
/// <see cref="OperatorActionEvent"/> through <see cref="IOperatorActionRecorder"/>.
/// Refusals surface as <see cref="DeadLetterRedriveOutcome"/> codes, never
/// silent no-ops.</para>
/// </summary>
public interface IDeadLetterRedriveService
{
    Task<DeadLetterRedriveResult> RedriveAsync(
        Guid eventId,
        string operatorIdentityId,
        string? rationale,
        CancellationToken cancellationToken = default);
}
