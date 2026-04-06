namespace Whycespace.Projections.Global.IntelligenceAccuracy;

public interface IIntelligenceAccuracyViewRepository
{
    Task SaveAsync(IntelligenceAccuracyReadModel model, CancellationToken ct = default);
    Task<IntelligenceAccuracyReadModel?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<IntelligenceAccuracyReadModel>> GetAllAsync(CancellationToken ct = default);
}
