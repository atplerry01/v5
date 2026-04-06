namespace Whycespace.Projections.Business.Agreement.Contract;

public interface IContractViewRepository
{
    Task SaveAsync(ContractReadModel model, CancellationToken ct = default);
    Task<ContractReadModel?> GetAsync(string id, CancellationToken ct = default);
}
