using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public sealed record StreamEndpoint : ValueObject
{
    public string Uri { get; }
    private StreamEndpoint(string uri) => Uri = uri;

    public static StreamEndpoint Create(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri) || !System.Uri.TryCreate(uri, UriKind.Absolute, out _))
            throw StreamSessionErrors.InvalidEndpoint();
        return new StreamEndpoint(uri.Trim());
    }

    public override string ToString() => Uri;
}
