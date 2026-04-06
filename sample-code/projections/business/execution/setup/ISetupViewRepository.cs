namespace Whycespace.Projections.Business.Execution.Setup;

public interface ISetupViewRepository
{
    Task SaveAsync(SetupReadModel model, CancellationToken ct = default);
    Task<SetupReadModel?> GetAsync(string id, CancellationToken ct = default);
}
