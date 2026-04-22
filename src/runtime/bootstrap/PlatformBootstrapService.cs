using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.Security;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.Bootstrap;

/// <summary>
/// Phase 2.8.25 — First-operator identity bootstrap.
///
/// Runs exactly once at host startup. Checks whether the first-operator
/// registration already exists in the event store (idempotent); if not,
/// dispatches <see cref="InitiateRegistrationCommand"/> so the host
/// never starts with no known operator.
///
/// The operator's <see cref="RegistryId"/> is derived deterministically
/// from the supplied email and registration type — same config always
/// produces the same ID, making restarts and multi-instance deploys safe.
///
/// Identity compliance (INV-202): establishes a <see cref="SystemIdentityScope"/>
/// with actor "system/platform-bootstrap" before dispatching so that
/// <c>HttpCallerIdentityAccessor</c> resolves a declared system identity
/// instead of throwing (no HTTP context at startup).
/// Policy compliance: routes through <c>DispatchSystemAsync</c>, which
/// passes the existing <c>whyce.trust.identity.registry.initiate</c>
/// WHYCEPOLICY binding.
/// </summary>
public sealed class PlatformBootstrapService : IHostedService
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly IEventStore _eventStore;
    private readonly ILogger<PlatformBootstrapService>? _logger;
    private readonly string _operatorEmail;
    private readonly string _registrationType;

    public PlatformBootstrapService(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IEventStore eventStore,
        string operatorEmail,
        string registrationType,
        ILogger<PlatformBootstrapService>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(operatorEmail))
            throw new ArgumentException(
                "operatorEmail must be non-empty. Set Bootstrap:FirstOperator:Email before starting the host.",
                nameof(operatorEmail));
        if (string.IsNullOrWhiteSpace(registrationType))
            throw new ArgumentException(
                "registrationType must be non-empty. Set Bootstrap:FirstOperator:RegistrationType before starting the host.",
                nameof(registrationType));

        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _eventStore = eventStore;
        _operatorEmail = operatorEmail;
        _registrationType = registrationType;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var registryId = _idGenerator.Generate(
            $"platform:bootstrap:first-operator:{_operatorEmail}:{_registrationType}");

        var existing = await _eventStore.LoadEventsAsync(registryId, cancellationToken);
        if (existing.Count > 0)
        {
            _logger?.LogInformation(
                "Platform bootstrap: first-operator registration {RegistryId} already exists ({Count} events). Skipping.",
                registryId, existing.Count);
            return;
        }

        _logger?.LogInformation(
            "Platform bootstrap: initiating first-operator registration {RegistryId} ({Email}, {RegistrationType}).",
            registryId, _operatorEmail, _registrationType);

        var command = new InitiateRegistrationCommand(
            registryId,
            _operatorEmail,
            _registrationType,
            _clock.UtcNow);

        var route = new DomainRoute("trust", "identity", "registry");

        using var scope = SystemIdentityScope.Begin(
            "system/platform-bootstrap",
            "system",
            "system:bootstrap");

        var result = await _dispatcher.DispatchSystemAsync(command, route, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Platform bootstrap failed: first-operator registration dispatch returned a failure. " +
                $"Error: {result.Error ?? "unknown"}");
        }

        _logger?.LogInformation(
            "Platform bootstrap: first-operator registration {RegistryId} successfully initiated.",
            registryId);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
