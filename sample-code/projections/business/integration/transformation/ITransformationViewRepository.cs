namespace Whycespace.Projections.Business.Integration.Transformation;

public interface ITransformationViewRepository
{
    Task SaveAsync(TransformationReadModel model, CancellationToken ct = default);
    Task<TransformationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
