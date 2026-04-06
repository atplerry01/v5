using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed record SPVCreatedEvent(Guid SPVId) : DomainEvent;
