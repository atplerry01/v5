namespace Whycespace.Projections.Structural.Cluster.Classification;

public interface IClassificationViewRepository
{
    Task SaveAsync(ClassificationReadModel model, CancellationToken ct = default);
    Task<ClassificationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
