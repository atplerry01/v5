namespace Whycespace.Shared.Contracts.Chain;

/// <summary>
/// Contract for chain sequence management.
/// Provides the current sequence state for chain anchoring.
/// </summary>
public interface IChainSequencer
{
    Task<long> GetNextSequenceAsync();
    Task<string> GetLastBlockHashAsync();
}
