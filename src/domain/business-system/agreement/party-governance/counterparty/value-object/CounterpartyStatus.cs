namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

// Intentional: starts Active on creation. A counterparty records an existing
// external party; it is not drafted before being recorded.
public enum CounterpartyStatus
{
    Active,
    Suspended,
    Terminated
}
