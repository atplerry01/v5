namespace Whycespace.Domain.EconomicSystem.Routing.Execution;

public sealed record EconomicFlowRoutedEvent(
    Guid SourceEntityId,
    Guid TargetEntityId,
    decimal Amount,
    string Currency) : DomainEvent;
