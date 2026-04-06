namespace Whycespace.Projections.Business.Agreement.Amendment;

public interface IAmendmentViewRepository
{
    Task SaveAsync(AmendmentReadModel model, CancellationToken ct = default);
    Task<AmendmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
