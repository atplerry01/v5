using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Runtime.Engine;

/// <summary>
/// Describes a registered engine with its metadata, command type, and version.
/// Used by the EngineResolver to enforce adapter-only registration and type safety.
/// </summary>
public sealed class EngineDescriptor
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required Type CommandType { get; init; }
    public required IEngine Engine { get; init; }

    /// <summary>
    /// Validates that the descriptor is well-formed and the engine is a TypedEngineAdapter.
    /// </summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(Version);
        ArgumentNullException.ThrowIfNull(CommandType);
        ArgumentNullException.ThrowIfNull(Engine);

        if (Engine is not ITypedEngineAdapter)
        {
            throw new InvalidOperationException(
                $"Engine '{Name}' must be wrapped in TypedEngineAdapter<T>. "
                + "Direct IEngine registration is forbidden.");
        }
    }
}
