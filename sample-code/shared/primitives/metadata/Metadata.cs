namespace Whycespace.Shared.Primitives.Metadata;

public sealed record Metadata
{
    private readonly IReadOnlyDictionary<string, string> _entries;

    public Metadata(IReadOnlyDictionary<string, string> entries)
    {
        _entries = entries ?? new Dictionary<string, string>();
    }

    public static Metadata Empty => new(new Dictionary<string, string>());

    public string? Get(string key) =>
        _entries.TryGetValue(key, out var value) ? value : null;

    public bool Has(string key) =>
        _entries.ContainsKey(key);

    public Metadata With(string key, string value)
    {
        var updated = new Dictionary<string, string>(_entries) { [key] = value };
        return new Metadata(updated);
    }

    public Metadata Without(string key)
    {
        var updated = new Dictionary<string, string>(_entries);
        updated.Remove(key);
        return new Metadata(updated);
    }

    public IReadOnlyDictionary<string, string> AsReadOnly() => _entries;
}
