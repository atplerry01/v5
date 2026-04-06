namespace Whycespace.Projections.Business.Document.ContractDocument;

public interface IContractDocumentViewRepository
{
    Task SaveAsync(ContractDocumentReadModel model, CancellationToken ct = default);
    Task<ContractDocumentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
