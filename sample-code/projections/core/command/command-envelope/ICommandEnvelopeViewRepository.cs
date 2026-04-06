namespace Whycespace.Projections.Core.Command.CommandEnvelope;

public interface ICommandEnvelopeViewRepository
{
    Task SaveAsync(CommandEnvelopeReadModel model, CancellationToken ct = default);
    Task<CommandEnvelopeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
