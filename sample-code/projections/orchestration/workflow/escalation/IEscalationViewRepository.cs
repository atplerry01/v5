namespace Whycespace.Projections.Orchestration.Workflow.Escalation;

public interface IEscalationViewRepository
{
    Task SaveAsync(EscalationReadModel model, CancellationToken ct = default);
    Task<EscalationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
