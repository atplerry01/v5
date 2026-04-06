namespace Whycespace.Projections.Business.Integration.Mapping;

public interface IMappingViewRepository
{
    Task SaveAsync(MappingReadModel model, CancellationToken ct = default);
    Task<MappingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
