namespace Whycespace.Projections.Core.Command.CommandCatalog;

public interface ICommandCatalogViewRepository
{
    Task SaveAsync(CommandCatalogReadModel model, CancellationToken ct = default);
    Task<CommandCatalogReadModel?> GetAsync(string id, CancellationToken ct = default);
}
