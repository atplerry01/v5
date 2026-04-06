namespace Whycespace.Projections.Business.Document.Record;

public interface IRecordViewRepository
{
    Task SaveAsync(RecordReadModel model, CancellationToken ct = default);
    Task<RecordReadModel?> GetAsync(string id, CancellationToken ct = default);
}
