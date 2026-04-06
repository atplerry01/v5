namespace Whycespace.Projections.Business.Document.Evidence;

public interface IEvidenceViewRepository
{
    Task SaveAsync(EvidenceReadModel model, CancellationToken ct = default);
    Task<EvidenceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
