namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Context for economic execution path optimization (E17.8).
/// Provides candidate paths for profit optimization and scenario simulation.
/// </summary>
public interface IOptimizationContext
{
    IEnumerable<Guid> CandidatePaths { get; }
}
