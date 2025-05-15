using CPMS.API.Handlers.ChargeSession;
using CPMS.API.Handlers.Connector;
using CPMS.BuildingBlocks.Domain;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Core.Models;
using CPMS.Core.Models.Enums;
using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Requests;
using CPMS.Core.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ProxyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILoggerService _logger;
    public ProxyController(IMediator mediator, ILoggerService logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AuthorizeChargerResponse>> Authorize(AuthorizeChargerRequest request)
    {
        _logger.Info($"Authorization request received for chargePoint: {request.OcppChargerId}, tagId: {request.IdTag}");
        
        try
        {
            var isAuthorized = await _mediator.Send(new AuthorizeTagCommand
            {
                TagId = request.IdTag
            });
            
            return Ok(new AuthorizeChargerResponse
            {
                AuthorizationStatus = isAuthorized ? AuthorizationStatus.Accepted : AuthorizationStatus.Invalid,
            });
        }
        catch (Exception ex)
        {
            _logger.Error($"Error authorizing tag: {request.IdTag} {ex.Message}");
            return StatusCode(500, "Internal server error during authorization");
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> StartTransaction(StartTransactionChargerResponse request)
    {
        _logger.Info($"Start transaction response from: {request.OcppChargerId}, TransactionId: {request.TransactionId}");
        
        try
        {
            var response = await _mediator.Send(new StartTransactionCommand
            {
                OcppChargerId = request.OcppChargerId,
                ConnectorId = request.OcppConnectorId,
                TagId = request.IdTag,
                MeterStart = request.MeterStart
            });
            
            return Ok(response);
        }
        catch (BusinessRuleValidationException ex)
        {
            _logger.Warning($"Business rule validation failed during start transaction for: {request.OcppChargerId} {ex.Message}");
            return BadRequest(new
            {
                Error = ex.Message,
                Details = ex.Details
            });
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing start transaction for: {request.OcppChargerId} {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<StopTransactionResponse>> StopTransaction(StopTransactionCpmsRequest request)
    {
        _logger.Info($"Stop transaction response TransactionId: {request.TranscationId}");
        
        try
        {
            if (request.TranscationId == null)
                throw new ArgumentNullException(nameof(request.TranscationId), "Transaction ID cannot be null");
            
            var stopTransactionResponse = await _mediator.Send(new StopTransactionCommand(
                request.TranscationId.Value,
                request.TimeStop,
                request.MeterStop,
                request.StopTagId,
                request.StopReason));
            
            return Ok(stopTransactionResponse);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing stop transaction for: {request.TranscationId} {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut]
    public async Task<ActionResult> BootNotification(BootNotificationRequest bootNotificationRequest)
    {
        _logger.Info($"Boot notification received from: {bootNotificationRequest.OcppChargerId}, " + $"Model: {bootNotificationRequest.ChargePointModel}, Vendor: {bootNotificationRequest.ChargePointVendor}");
        
        try
        {
            var success = await _mediator.Send(new BootNotificationCommand(bootNotificationRequest));
            
            if (success)
            {
                return Ok(success);
            }
            
            return BadRequest(success);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing boot notification for: {bootNotificationRequest.OcppChargerId} {ex.Message}");
            return StatusCode(500, "Internal server error during boot notification");
        }
    }
    
    [HttpPut]
    public async Task<ActionResult> MeterValues(MeterValuesRequest meterValuesRequest)
    {
        _logger.Info($"MeterValues request received from: {meterValuesRequest.OcppChargerId}, TransactionId: {meterValuesRequest.TransactionId}");
        
        try
        {

            await _mediator.Send(new MeterValuesCommand(meterValuesRequest));
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing meter values for: {meterValuesRequest.OcppChargerId}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut]
    public async Task<ActionResult> StatusNotification(StatusNotificationRequest request)
    {
        _logger.Info($"Status notification from: {((BaseMessage)request).OcppChargerId}, " +
                     $"Connector: {request.OcppConnectorId}, Status: {request.LastStatus}");
        
        try
        {
            await _mediator.Send(new UpdateConnectorStatusCommand
            {
                OcppChargerId = request.OcppChargerId,
                ConnectorId = request.OcppConnectorId,
                Status = request.LastStatus,
                Timestamp = request.LastStatusTime
            });
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error( $"Error processing status notification for: {((BaseMessage)request).OcppChargerId}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    //TODO: Uncomment and implement these methods as needed
    /*public async Task<ActionResult> ConnectorUnlocked(ConnectorUnlockedResponse response)
    {
        _logger.Info($"Connector unlocked response from: {response.ChargerId}, Status: {response.Status}");
        
        try
        {
            await _mediator.Send(new ProcessConnectorUnlockResponseCommand
            {
                ChargerId = response.ChargerId,
                ConnectorId = response.ConnectorId,
                Status = response.Status
            });
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing connector unlocked response for: {response.ChargerId}");
            return StatusCode(500, "Internal server error");
        }
    }*/
    /*
    public async Task<ActionResult> ChargingProfileCleared(ClearChargingProfileResponse response)
    {
        _logger.Info($"Charging profile cleared for: {response.ChargerId}, Status: {response.Status}");
        
        try
        {
            await _mediator.Send(new ClearChargingProfileCommand
            {
                ChargerId = response.ChargerId,
                Status = response.Status
            });
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing charging profile cleared for: {response.ChargerId} {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    public async Task<ActionResult> Reset(ResetChargerResponse resetResponse)
    {
        _logger.Info($"Reset response from: {resetResponse.ChargerId}, Status: {resetResponse.Status}");
        
        try
        {
            await _mediator.Send(new ProcessResetResponseCommand
            {
                ChargerId = resetResponse.ChargerId,
                Status = resetResponse.Status
            });
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing reset response for: {resetResponse.ChargerId}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    public async Task<ActionResult> ChargingProfileSet(SetChargingProfileResponse setChargingProfileResponse)
    {
        _logger.Info($"Charging profile set response from: {setChargingProfileResponse.ChargerId}, Status: {setChargingProfileResponse.Status}");
        
        try
        {
            await _mediator.Send(new ProcessSetChargingProfileResponseCommand
            {
                ChargerId = setChargingProfileResponse.ChargerId,
                Status = setChargingProfileResponse.Status
            });
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing charging profile set response for: {setChargingProfileResponse.ChargerId}");
            return StatusCode(500, "Internal server error");
        }
    }*/
}