using System.Text;

namespace Whycespace.Engines.T0U.WhycePolicy.Opa;

/// <summary>
/// Stateless HTTP adapter for Open Policy Agent.
/// Pure execution — no business logic.
/// </summary>
public sealed class OpaClient
{
    private readonly HttpClient _httpClient;

    public OpaClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<OpaResponse> EvaluateAsync(
        string endpoint,
        string requestBody,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestBody);

        using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return OpaResponse.Error(
                    (int)response.StatusCode,
                    $"OPA returned HTTP {(int)response.StatusCode}");
            }

            return OpaResponse.Success(body);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return OpaResponse.Error(0, $"OPA unreachable: {ex.Message}");
        }
    }
}

public sealed record OpaResponse
{
    public required bool IsSuccess { get; init; }
    public string? Body { get; init; }
    public int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static OpaResponse Success(string body) => new()
    {
        IsSuccess = true, Body = body, StatusCode = 200
    };

    public static OpaResponse Error(int statusCode, string message) => new()
    {
        IsSuccess = false, StatusCode = statusCode, ErrorMessage = message
    };
}
