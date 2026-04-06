namespace Whycespace.Projections.Decision.Risk.IncidentRisk;

public interface IIncidentRiskViewRepository
{
    Task SaveAsync(IncidentRiskReadModel model, CancellationToken ct = default);
    Task<IncidentRiskReadModel?> GetAsync(string id, CancellationToken ct = default);
}
