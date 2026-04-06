namespace Whycespace.Projections.Business.Integration.Export;

public interface IExportViewRepository
{
    Task SaveAsync(ExportReadModel model, CancellationToken ct = default);
    Task<ExportReadModel?> GetAsync(string id, CancellationToken ct = default);
}
