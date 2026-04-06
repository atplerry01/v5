namespace Whycespace.Projections.Business.Integration.Contract;

public interface IContractViewRepository
{
    Task SaveAsync(ContractReadModel model, CancellationToken ct = default);
    Task<ContractReadModel?> GetAsync(string id, CancellationToken ct = default);
}
