namespace Whycespace.Domain.BusinessSystem.Integration.Synchronization;

public sealed record SynchronizationCreatedEvent(SynchronizationId SynchronizationId, SyncPolicyId PolicyId);
