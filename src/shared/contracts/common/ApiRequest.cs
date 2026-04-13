namespace Whycespace.Shared.Contracts.Common;

/// <summary>
/// Standard API request envelope. POST endpoints accept this shape.
/// The Data property carries the domain-specific request payload.
/// Meta is optional — callers may supply a CorrelationId or RequestId
/// for traceability; otherwise the runtime generates them.
/// </summary>
public sealed class ApiRequest<T>
{
    public RequestMeta? Meta { get; set; }
    public T Data { get; set; } = default!;
}
