namespace Whycespace.Projections.Business.Logistic.Handoff;

public interface IHandoffViewRepository
{
    Task SaveAsync(HandoffReadModel model, CancellationToken ct = default);
    Task<HandoffReadModel?> GetAsync(string id, CancellationToken ct = default);
}
