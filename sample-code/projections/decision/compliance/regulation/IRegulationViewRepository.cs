namespace Whycespace.Projections.Decision.Compliance.Regulation;

public interface IRegulationViewRepository
{
    Task SaveAsync(RegulationReadModel model, CancellationToken ct = default);
    Task<RegulationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
