namespace Whycespace.Projections.Business.Agreement.Signature;

public interface ISignatureViewRepository
{
    Task SaveAsync(SignatureReadModel model, CancellationToken ct = default);
    Task<SignatureReadModel?> GetAsync(string id, CancellationToken ct = default);
}
