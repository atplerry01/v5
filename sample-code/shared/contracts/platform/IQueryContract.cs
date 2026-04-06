namespace Whycespace.Shared.Contracts.Platform;

/// <summary>
/// Defines the contract boundary for query handling in the Platform API layer.
/// Implementations translate between platform DTOs and projection read models.
/// Must never trigger side effects or expose domain entities.
/// </summary>
public interface IQueryContract
{
    string QueryType { get; }
    int ContractVersion { get; }

    /// <summary>Validates the query payload before dispatching to the projection.</summary>
    bool CanHandle(QueryRequestDTO request);

    /// <summary>Executes the query, returning a response envelope with serialized projection data.</summary>
    Task<QueryResponseDTO> ExecuteAsync(QueryRequestDTO request, CancellationToken ct = default);
}
