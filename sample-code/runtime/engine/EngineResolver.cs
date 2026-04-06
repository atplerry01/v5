using System.Collections.Concurrent;
using Whycespace.Runtime.ControlPlane;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Runtime.Engine;

public sealed class EngineResolver : IEngineRegistry
{
    private readonly ConcurrentDictionary<string, EngineDescriptor> _descriptors = new();
    private volatile bool _isLocked;

    /// <summary>
    /// Registers an engine via descriptor. Enforces adapter-only registration.
    /// </summary>
    public void Register(EngineDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (_isLocked)
            throw new RuntimeControlPlaneException(
                "Engine registry is locked. Registration is only allowed during startup.",
                "ENGINE_REGISTRY_LOCKED");

        descriptor.Validate();

        if (!_descriptors.TryAdd(descriptor.Name, descriptor))
            throw new RuntimeControlPlaneException(
                $"Engine '{descriptor.Name}' is already registered. Duplicate registration is forbidden.",
                "ENGINE_ALREADY_REGISTERED");
    }

    /// <summary>
    /// Registers an engine with automatic descriptor creation.
    /// The engine MUST be a TypedEngineAdapter.
    /// </summary>
    public void Register(string commandType, IEngine engine)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandType);
        ArgumentNullException.ThrowIfNull(engine);

        if (engine is not ITypedEngineAdapter adapter)
            throw new RuntimeControlPlaneException(
                $"Engine for '{commandType}' must be a TypedEngineAdapter<T>. "
                + "Direct IEngine registration is forbidden.",
                "ENGINE_NOT_ADAPTER");

        Register(new EngineDescriptor
        {
            Name = commandType,
            Version = "v1",
            CommandType = adapter.CommandType,
            Engine = engine
        });
    }

    public IEngine? Resolve(string commandType)
    {
        return _descriptors.TryGetValue(commandType, out var descriptor)
            ? descriptor.Engine
            : null;
    }

    public EngineDescriptor? ResolveDescriptor(string commandType)
    {
        _descriptors.TryGetValue(commandType, out var descriptor);
        return descriptor;
    }

    public bool HasEngine(string commandType) => _descriptors.ContainsKey(commandType);

    /// <summary>
    /// Locks the registry. No further registrations are allowed after this call.
    /// Called automatically by RuntimeControlPlaneBuilder.Build().
    /// </summary>
    public void Lock()
    {
        _isLocked = true;
    }

    public bool IsLocked => _isLocked;
    public int Count => _descriptors.Count;

    public IReadOnlyList<EngineDescriptor> GetAll() => [.. _descriptors.Values];
}
