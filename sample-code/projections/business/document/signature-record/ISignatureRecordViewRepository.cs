namespace Whycespace.Projections.Business.Document.SignatureRecord;

public interface ISignatureRecordViewRepository
{
    Task SaveAsync(SignatureRecordReadModel model, CancellationToken ct = default);
    Task<SignatureRecordReadModel?> GetAsync(string id, CancellationToken ct = default);
}
