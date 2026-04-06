namespace Whycespace.Projections.Decision.Compliance.Obligation;

public interface IObligationViewRepository
{
    Task SaveAsync(ObligationReadModel model, CancellationToken ct = default);
    Task<ObligationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
