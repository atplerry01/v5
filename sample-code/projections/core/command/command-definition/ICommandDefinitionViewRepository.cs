namespace Whycespace.Projections.Core.Command.CommandDefinition;

public interface ICommandDefinitionViewRepository
{
    Task SaveAsync(CommandDefinitionReadModel model, CancellationToken ct = default);
    Task<CommandDefinitionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
