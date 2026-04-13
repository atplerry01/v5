namespace Whycespace.Shared.Contracts.Infrastructure.Projection;

public interface IProjectionHandler<in TEvent> where TEvent : class
{
    // phase1.5-S5.2.3 / TC-6 (PROJECTION-CT-CONTRACT-01): typed projection
    // handler now consumes the worker's stoppingToken so a hung handler
    // can be unblocked at the database round-trip without waiting for
    // Kafka poll/session limits to intervene.
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
