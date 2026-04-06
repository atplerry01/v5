namespace Whycespace.Projections.Business.Integration.Import;

public interface IImportViewRepository
{
    Task SaveAsync(ImportReadModel model, CancellationToken ct = default);
    Task<ImportReadModel?> GetAsync(string id, CancellationToken ct = default);
}
