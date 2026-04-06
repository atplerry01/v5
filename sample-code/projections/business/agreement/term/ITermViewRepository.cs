namespace Whycespace.Projections.Business.Agreement.Term;

public interface ITermViewRepository
{
    Task SaveAsync(TermReadModel model, CancellationToken ct = default);
    Task<TermReadModel?> GetAsync(string id, CancellationToken ct = default);
}
