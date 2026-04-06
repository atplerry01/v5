namespace Whycespace.Projections.Operational.Global.IncidentResponse;

public interface IIncidentResponseViewRepository
{
    Task SaveAsync(IncidentResponseReadModel model, CancellationToken ct = default);
    Task<IncidentResponseReadModel?> GetAsync(string id, CancellationToken ct = default);
}
