using System.Text.Json;
using Whycespace.Platform.Host.Adapters.OutboundEffects;
using Whycespace.Shared.Contracts.EventFabric;
using Xunit;

namespace Whycespace.Tests.Integration.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.3 / R-OUT-EFF-QUEUE-PAYLOAD-REGISTRY-01 — validates payload
/// serialization / deserialization through <see cref="IPayloadTypeRegistry"/>
/// with fail-closed semantics (option (a)). No Postgres dependency — the
/// round-trip is exercised via reflection on the private Serialize/Deserialize
/// helpers because the real claim/insert path requires a live datasource.
/// </summary>
public sealed class PostgresOutboundEffectQueueStorePayloadRegistryTests
{
    private static (Func<object, string> serialize, Func<string, object> deserialize)
        ReflectHelpers(IPayloadTypeRegistry registry)
    {
        // The store needs an EventStoreDataSource for ctor validation but we
        // never touch the pool since we're only exercising the private
        // serialize/deserialize helpers. Skip the ctor via
        // RuntimeHelpers.GetUninitializedObject so we can stub the private
        // _payloadTypes field directly.
        var store = (PostgresOutboundEffectQueueStore)
            System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(typeof(PostgresOutboundEffectQueueStore));
        var field = typeof(PostgresOutboundEffectQueueStore).GetField(
            "_payloadTypes",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field!.SetValue(store, registry);

        var serialize = typeof(PostgresOutboundEffectQueueStore).GetMethod(
            "SerializePayload",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var deserialize = typeof(PostgresOutboundEffectQueueStore).GetMethod(
            "DeserializePayload",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        return (
            payload => (string)serialize!.Invoke(store, new[] { payload })!,
            json => deserialize!.Invoke(store, new object[] { json })!);
    }

    [Fact]
    public void Round_trip_preserves_registered_payload()
    {
        var registry = new StubRegistry();
        registry.Register(typeof(TestPayload));
        var (serialize, deserialize) = ReflectHelpers(registry);

        var payload = new TestPayload("hello", 42);
        var json = serialize(payload);
        var roundTripped = deserialize(json);

        var asTest = Assert.IsType<TestPayload>(roundTripped);
        Assert.Equal("hello", asTest.Message);
        Assert.Equal(42, asTest.Count);
    }

    [Fact]
    public void Serialize_fails_closed_for_unregistered_type()
    {
        var registry = new StubRegistry(); // no Register() call
        var (serialize, _) = ReflectHelpers(registry);

        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            serialize(new TestPayload("x", 1)));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("not registered", ex.InnerException!.Message);
    }

    [Fact]
    public void Deserialize_fails_closed_for_unknown_type_name()
    {
        var registry = new StubRegistry();
        var (_, deserialize) = ReflectHelpers(registry);

        var envelope = JsonSerializer.Serialize(new { TypeName = "Unknown.Type", Json = "{}" });

        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
            deserialize(envelope));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public void Serialized_envelope_uses_registry_short_name_not_assembly_qualified_name()
    {
        var registry = new StubRegistry();
        registry.Register(typeof(TestPayload));
        var (serialize, _) = ReflectHelpers(registry);

        var json = serialize(new TestPayload("x", 1));

        // The registry's canonical name for TestPayload is its simple name,
        // NOT the AQN. The envelope must carry that short name — this is
        // the R3.B.3 discipline swap.
        var envelope = JsonSerializer.Deserialize<EnvelopeProbe>(json);
        Assert.NotNull(envelope);
        Assert.Equal("TestPayload", envelope!.TypeName);
        Assert.DoesNotContain("Version=", envelope.TypeName);
    }

    private sealed record TestPayload(string Message, int Count);

    private sealed record EnvelopeProbe(string TypeName, string Json);

    private sealed class StubRegistry : IPayloadTypeRegistry
    {
        private readonly Dictionary<string, Type> _byName = new(StringComparer.Ordinal);
        private readonly Dictionary<Type, string> _byType = new();

        public void Register(Type type)
        {
            var name = type.Name;
            _byName[name] = type;
            _byType[type] = name;
        }
        public void Register<T>() => Register(typeof(T));
        public bool TryGetName(Type type, out string? name)
        {
            if (_byType.TryGetValue(type, out var n)) { name = n; return true; }
            name = null; return false;
        }
        public Type Resolve(string typeName) =>
            _byName.TryGetValue(typeName, out var t)
                ? t
                : throw new InvalidOperationException($"Unknown payload type '{typeName}'.");
    }
}
