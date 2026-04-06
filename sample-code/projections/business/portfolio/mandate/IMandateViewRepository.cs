namespace Whycespace.Projections.Business.Portfolio.Mandate;

public interface IMandateViewRepository
{
    Task SaveAsync(MandateReadModel model, CancellationToken ct = default);
    Task<MandateReadModel?> GetAsync(string id, CancellationToken ct = default);
}
