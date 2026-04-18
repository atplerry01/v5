using System.Net.Http.Json;
using System.Text.Json;
using Whycespace.Shared.Contracts.Common;

namespace Whycespace.Tests.E2E.Economic.Revenue.Setup;

public static class RevenuePipelineApiEnvelope
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static Task<HttpResponseMessage> PostAsync<TPayload>(
        HttpClient http, string path, TPayload payload, Guid correlationId, CancellationToken ct = default)
    {
        var envelope = new ApiRequest<TPayload>
        {
            Meta = new RequestMeta { CorrelationId = correlationId.ToString() },
            Data = payload
        };
        return http.PostAsJsonAsync(path, envelope, Json, ct);
    }

    public static async Task<ApiResponse<TData>?> ReadAsync<TData>(HttpResponseMessage response, CancellationToken ct = default)
    {
        var raw = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return JsonSerializer.Deserialize<ApiResponse<TData>>(raw, Json);
    }
}
