namespace Whycespace.Projections.Business.Integration.CommandBridge;

public interface ICommandBridgeViewRepository
{
    Task SaveAsync(CommandBridgeReadModel model, CancellationToken ct = default);
    Task<CommandBridgeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
