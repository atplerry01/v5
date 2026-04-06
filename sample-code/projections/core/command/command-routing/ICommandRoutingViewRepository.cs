namespace Whycespace.Projections.Core.Command.CommandRouting;

public interface ICommandRoutingViewRepository
{
    Task SaveAsync(CommandRoutingReadModel model, CancellationToken ct = default);
    Task<CommandRoutingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
