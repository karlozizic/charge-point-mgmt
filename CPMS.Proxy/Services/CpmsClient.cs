using System.Net.Http.Json;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Models;
using CPMS.Proxy.Models.Cpms.Requests;
using CPMS.Proxy.Models.Cpms.Responses;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;

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

    public async Task<AuthorizeChargerResponse> Authorize(AuthorizeChargerCpmsRequest authorizeChargerCpmsRequest)
    {
        try
        {
            _loggerService.Info($"Sending request {JsonConvert.SerializeObject(authorizeChargerCpmsRequest)} to CPMS API");
            
            var response = await _client.PostAsJsonAsync(
                $"{ApiPath}/Authorize", 
                authorizeChargerCpmsRequest);

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

    public async Task BootNotification(BootNotificationCpmsRequest bootNotificationRequest)
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

    public async Task ChargingProfileCleared(ClearChargingProfileCpmsResponse response)
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

    public async Task MeterValues(MeterValuesCpmsRequest meterValuesRequest)
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

    public async Task ChargingProfileSet(SetChargingProfileCpmsResponse setChargingProfileResponse)
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

    public async Task StatusNotification(StatusNotificationCpmsRequest statusNotificationRequest)
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

    public async Task<StopTransactionResponse> StopTransaction(StopTransactionCpmsResponse response)
    {
        try
        {
            _loggerService.Info($"Sending request {response} to CPMS API");
            var stopTransactionResponse = await _client.PostAsJsonAsync($"{ApiPath}/StopTransaction", response);
            _loggerService.Info($"Received response {await stopTransactionResponse.Content.ReadAsStringAsync()} from CPMS API");
            return await stopTransactionResponse.Content.ReadFromJsonAsync<StopTransactionResponse>() ?? throw new InvalidOperationException();
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
    Task<AuthorizeChargerResponse> Authorize(AuthorizeChargerCpmsRequest authorizeChargerCpmsRequest);
    Task BootNotification(BootNotificationCpmsRequest bootNotificationRequest);
    Task ChargingProfileCleared(ClearChargingProfileCpmsResponse response);
    Task MeterValues(MeterValuesCpmsRequest meterValuesRequest);
    Task Reset(ResetChargerResponse resetResponse);
    Task ChargingProfileSet(SetChargingProfileCpmsResponse setChargingProfileResponse);
    Task<StartTransactionResponse> StartTransaction(StartTransactionChargerResponse response);
    Task StatusNotification(StatusNotificationCpmsRequest statusNotificationRequest);
    Task<StopTransactionResponse> StopTransaction(StopTransactionCpmsResponse response);
    Task ConnectorUnlocked(ConnectorUnlockedResponse response);
}