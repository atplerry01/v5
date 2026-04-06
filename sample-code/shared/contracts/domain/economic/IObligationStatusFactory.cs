namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Engine-facing obligation status creation contract.
/// Engines MUST NOT construct domain ObligationStatus directly.
/// </summary>
public interface IObligationStatusFactory
{
    object Create(string status);
}
