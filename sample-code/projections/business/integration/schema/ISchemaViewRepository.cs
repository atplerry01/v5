namespace Whycespace.Projections.Business.Integration.Schema;

public interface ISchemaViewRepository
{
    Task SaveAsync(SchemaReadModel model, CancellationToken ct = default);
    Task<SchemaReadModel?> GetAsync(string id, CancellationToken ct = default);
}
