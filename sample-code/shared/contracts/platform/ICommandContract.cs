namespace Whycespace.Shared.Contracts.Platform;

/// <summary>
/// Defines the contract boundary for command handling in the Platform API layer.
/// Implementations translate between platform DTOs and domain operations.
/// Enforces the Command → Workflow → Response flow.
/// </summary>
public interface ICommandContract
{
    string CommandType { get; }
    int ContractVersion { get; }

    /// <summary>Validates the command payload before dispatching to the domain.</summary>
    bool CanHandle(CommandRequestDTO request);

    /// <summary>Executes the command, returning a response envelope. Must not expose domain internals.</summary>
    Task<CommandResponseDTO> ExecuteAsync(CommandRequestDTO request, CancellationToken ct = default);
}
