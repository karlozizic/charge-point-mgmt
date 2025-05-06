using System.Net.Http.Json;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Core.Models.Requests;
using CPMS.Core.Models.Responses;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;
using BootNotificationRequest = CPMS.Core.Models.Requests.BootNotificationRequest;
using ClearChargingProfileResponse = CPMS.Core.Models.Responses.ClearChargingProfileResponse;
using MeterValuesRequest = CPMS.Core.Models.Requests.MeterValuesRequest;
using SetChargingProfileResponse = CPMS.Core.Models.Responses.SetChargingProfileResponse;
using StatusNotificationRequest = CPMS.Core.Models.Requests.StatusNotificationRequest;
using StopTransactionResponse = CPMS.Core.Models.Responses.StopTransactionResponse;

namespace CPMS.Proxy.Services;

public class CpmsClient : ICpmsClient
{
    private readonly HttpClient _client;
    private readonly ILoggerService _loggerService;
    private const string ApiPath = "api/Proxy";

    public CpmsClient(
        HttpClient client,
        ILoggerService loggerService)
    {
        _client = client;
        _loggerService = loggerService;
    }

    public async Task<AuthorizeChargerResponse> Authorize(AuthorizeChargerRequest authorizeChargerRequest)
    {
        try
        {
            _loggerService.Info($"Sending request {JsonConvert.SerializeObject(authorizeChargerRequest)} to CPMS API");
            
            var response = await _client.PostAsJsonAsync(
                $"{ApiPath}/Authorize", 
                authorizeChargerRequest);

            response.EnsureSuccessStatusCode();

            var authorizationResponse = await response.Content.ReadFromJsonAsync<AuthorizeChargerResponse>();

            _loggerService.Info($"Received response {JsonConvert.SerializeObject(authorizationResponse)} from CPMS API");

            return authorizationResponse!;
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task BootNotification(BootNotificationRequest bootNotificationRequest)
    {
        try
        {
            _loggerService.Info($"Sending request {bootNotificationRequest} to CPMS API");
            var response = await _client.PutAsJsonAsync($"{ApiPath}/BootNotification", bootNotificationRequest);
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task ChargingProfileCleared(ClearChargingProfileResponse response)
    {
        try
        {
            _loggerService.Info($"Sending request {response} to CPMS API");
            await _client.PostAsJsonAsync($"{ApiPath}/ChargingProfileCleared", response);   
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task MeterValues(MeterValuesRequest meterValuesRequest)
    {
        try
        {
            _loggerService.Info($"Sending request {meterValuesRequest} to CPMS API");
            await _client.PutAsJsonAsync($"{ApiPath}/MeterValues", meterValuesRequest);
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task Reset(ResetChargerResponse resetResponse)
    {
        try
        {
            _loggerService.Info($"Sending request {resetResponse} to CPMS API");
            await _client.PostAsJsonAsync($"{ApiPath}/Reset", resetResponse);
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task ChargingProfileSet(SetChargingProfileResponse setChargingProfileResponse)
    {
        try
        {
            _loggerService.Info($"Sending request {setChargingProfileResponse} to CPMS API");
            await _client.PostAsJsonAsync($"{ApiPath}/ChargingProfileSet", setChargingProfileResponse);
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task<StartTransactionResponse> StartTransaction(StartTransactionChargerResponse response)
    {
        try
        {
            _loggerService.Info($"Sending request {response} to CPMS API");
            var startTransactionResponse = await _client.PostAsJsonAsync($"{ApiPath}/StartTransaction", response);
            _loggerService.Info($"Received response {await startTransactionResponse.Content.ReadAsStringAsync()} from CPMS API");
            return await startTransactionResponse.Content.ReadFromJsonAsync<StartTransactionResponse>() ?? throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task StatusNotification(StatusNotificationRequest statusNotificationRequest)
    {
        try
        {
            _loggerService.Info($"Sending request {statusNotificationRequest} to CPMS API");
            await _client.PutAsJsonAsync($"{ApiPath}/StatusNotification", statusNotificationRequest);
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task<OCPP_1._6.StopTransactionResponse> StopTransaction(StopTransactionResponse response)
    {
        try
        {
            _loggerService.Info($"Sending request {response} to CPMS API");
            var stopTransactionResponse = await _client.PostAsJsonAsync($"{ApiPath}/StopTransaction", response);
            _loggerService.Info($"Received response {await stopTransactionResponse.Content.ReadAsStringAsync()} from CPMS API");
            return await stopTransactionResponse.Content.ReadFromJsonAsync<OCPP_1._6.StopTransactionResponse>() ?? throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }

    public async Task ConnectorUnlocked(ConnectorUnlockedResponse response)
    {
        try
        {
            _loggerService.Info($"Sending request {response} to CPMS API");
            await _client.PostAsJsonAsync($"{ApiPath}/ConnectorUnlocked", response);
        }
        catch (Exception ex)
        {
            _loggerService.Error(ex.Message);
            throw;
        }
    }
}

public interface ICpmsClient
{
    Task<AuthorizeChargerResponse> Authorize(AuthorizeChargerRequest authorizeChargerRequest);
    Task BootNotification(BootNotificationRequest bootNotificationRequest);
    Task ChargingProfileCleared(ClearChargingProfileResponse response);
    Task MeterValues(MeterValuesRequest meterValuesRequest);
    Task Reset(ResetChargerResponse resetResponse);
    Task ChargingProfileSet(SetChargingProfileResponse setChargingProfileResponse);
    Task<StartTransactionResponse> StartTransaction(StartTransactionChargerResponse response);
    Task StatusNotification(StatusNotificationRequest statusNotificationRequest);
    Task<OCPP_1._6.StopTransactionResponse> StopTransaction(StopTransactionResponse response);
    Task ConnectorUnlocked(ConnectorUnlockedResponse response);
}