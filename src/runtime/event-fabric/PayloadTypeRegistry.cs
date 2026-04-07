using System.Collections.Concurrent;
using Whyce.Shared.Contracts.EventFabric;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Generic, domain-agnostic implementation of <see cref="IPayloadTypeRegistry"/>.
/// Mirrors the registration shape of <see cref="EventSchemaRegistry"/>: bootstrap
/// modules call <c>Register</c> from inside the DI factory closure before the
/// registry is locked, after which it is read-only.
///
/// Per runtime.guard rule 11.R-DOM-01 this class holds no domain references —
/// the map is keyed on the unqualified CLR type name string. The chosen key
/// (<see cref="System.Type.FullName"/>) is fully qualified by namespace and
/// nested-type chain so collisions across domains are not possible.
/// </summary>
public sealed class PayloadTypeRegistry : IPayloadTypeRegistry
{
    private readonly ConcurrentDictionary<string, System.Type> _byName = new();
    private bool _locked;

    public void Register(System.Type type)
    {
        if (_locked)
            throw new InvalidOperationException("PayloadTypeRegistry is locked. Cannot register after build.");

        var name = NameOf(type);
        _byName[name] = type;
    }

    public void Register<T>() => Register(typeof(T));

    public bool TryGetName(System.Type type, out string? name)
    {
        var candidate = NameOf(type);
        if (_byName.ContainsKey(candidate))
        {
            name = candidate;
            return true;
        }

        name = null;
        return false;
    }

    public System.Type Resolve(string typeName)
    {
        if (!_byName.TryGetValue(typeName, out var type))
            throw new InvalidOperationException(
                $"PayloadTypeRegistry has no entry for '{typeName}'. " +
                $"Register the type from the owning IDomainBootstrapModule.RegisterPayloadTypes.");

        return type;
    }

    public void Lock() => _locked = true;

    private static string NameOf(System.Type type) =>
        type.FullName ?? type.Name;
}
