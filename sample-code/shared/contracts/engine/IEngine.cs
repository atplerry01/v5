namespace Whycespace.Shared.Contracts.Engine;

public interface IEngine
{
    string EngineId { get; }
    Task<EngineResult> ExecuteAsync(EngineRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Typed engine contract for domain-specific command handling.
/// Implementations are bridged to the runtime's IEngine via TypedEngineAdapter.
/// </summary>
public interface IEngine<in TCommand> where TCommand : notnull
{
    Task<EngineResult> ExecuteAsync(TCommand command, EngineContext context, CancellationToken cancellationToken = default);
}
