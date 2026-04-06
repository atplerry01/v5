namespace Whycespace.Domain.CoreSystem.State.SystemState;

/// <summary>
/// Domain service for system state evaluation.
/// Stateless — all data passed as parameters.
/// </summary>
public sealed class SystemStateService
{
    public bool IsAuthoritative(SystemStateAggregate state) =>
        state.Status == SystemStateStatus.Authoritative;

    public bool CanDeclareAuthoritative(SystemStateAggregate state) =>
        state.Status == SystemStateStatus.Validating
        && state.CurrentSnapshot is not null
        && state.Validations.Count > 0
        && state.Validations.All(v => v.IsValid);

    public bool IsDegraded(SystemStateAggregate state) =>
        state.Status == SystemStateStatus.Degraded;
}
