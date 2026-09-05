using System.Net;
using System.Net.Http.Json;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Requests;
using CPMS.Core.Models.Responses;

namespace CPMS.Proxy.Services;

/// <summary>HTTP client for the API's ProxyController. Known gap: no retries, no circuit breaker, 120 s timeout.</summary>
public interface ICpmsClient
{
    Task<AuthorizeChargerResponse> Authorize(AuthorizeChargerRequest request);

    /// <summary>True when the API knows the charger; false when it is not registered.</summary>
    Task<bool> BootNotification(BootNotificationRequest request);

    Task<StartTransactionResponse> StartTransaction(StartTransactionChargerResponse request);
    Task<StopTransactionResponse> StopTransaction(StopTransactionCpmsRequest request);
    Task MeterValues(MeterValuesRequest request);
    Task StatusNotification(StatusNotificationRequest request);
}

public class CpmsClient : ICpmsClient
{
    private const string ApiPath = "api/Proxy";

    private readonly HttpClient _client;
    private readonly ILoggerService _logger;

    public CpmsClient(HttpClient client, ILoggerService logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<AuthorizeChargerResponse> Authorize(AuthorizeChargerRequest request)
    {
        using var response = await Send(HttpMethod.Post, "Authorize", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthorizeChargerResponse>()
               ?? throw new InvalidOperationException("Empty Authorize response");
    }

    public async Task<bool> BootNotification(BootNotificationRequest request)
    {
        using var response = await Send(HttpMethod.Put, "BootNotification", request);
        if (response.StatusCode == HttpStatusCode.BadRequest)
            return false;

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<StartTransactionResponse> StartTransaction(StartTransactionChargerResponse request)
    {
        using var response = await Send(HttpMethod.Post, "StartTransaction", request);

        // The API answers 400 when the tag is invalid, blocked or expired.
        if (response.StatusCode == HttpStatusCode.BadRequest)
            return new StartTransactionResponse { TransactionId = 0, IdTagInfo = Invalid() };

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StartTransactionResponse>()
               ?? throw new InvalidOperationException("Empty StartTransaction response");
    }

    public async Task<StopTransactionResponse> StopTransaction(StopTransactionCpmsRequest request)
    {
        using var response = await Send(HttpMethod.Post, "StopTransaction", request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return new StopTransactionResponse { IdTagInfo = Invalid() };

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StopTransactionResponse>()
               ?? throw new InvalidOperationException("Empty StopTransaction response");
    }

    public async Task MeterValues(MeterValuesRequest request)
    {
        using var response = await Send(HttpMethod.Put, "MeterValues", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task StatusNotification(StatusNotificationRequest request)
    {
        using var response = await Send(HttpMethod.Put, "StatusNotification", request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> Send<T>(HttpMethod method, string action, T body)
    {
        using var httpRequest = new HttpRequestMessage(method, $"{ApiPath}/{action}")
        {
            Content = JsonContent.Create(body)
        };

        var response = await _client.SendAsync(httpRequest);
        _logger.Info($"CPMS API {action} => {(int)response.StatusCode}");
        return response;
    }

    private static IdTagInfo Invalid() => new() { Status = AuthorizationStatus.Invalid };
}
