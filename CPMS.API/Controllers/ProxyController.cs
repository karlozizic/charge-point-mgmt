using CPMS.API.Handlers.ChargePoint;
using CPMS.API.Handlers.ChargeSession;
using CPMS.API.Handlers.Connector;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Requests;
using CPMS.Core.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Controllers;

/// <summary>Endpoints called by the OCPP gateway (CPMS.Proxy). Errors are mapped by ApiExceptionHandler.</summary>
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

        var isAuthorized = await _mediator.Send(new AuthorizeTagCommand { TagId = request.IdTag });

        return Ok(new AuthorizeChargerResponse
        {
            AuthorizationStatus = isAuthorized ? AuthorizationStatus.Accepted : AuthorizationStatus.Invalid
        });
    }

    [HttpPost]
    public async Task<ActionResult<StartTransactionResponse>> StartTransaction(StartTransactionChargerResponse request)
    {
        _logger.Info($"Start transaction from: {request.OcppChargerId}");

        var response = await _mediator.Send(new StartTransactionCommand
        {
            OcppChargerId = request.OcppChargerId,
            ConnectorId = request.OcppConnectorId,
            TagId = request.IdTag,
            MeterStart = request.MeterStart
        });

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<StopTransactionResponse>> StopTransaction(StopTransactionCpmsRequest request)
    {
        _logger.Info($"Stop transaction for TransactionId: {request.TranscationId}");

        if (request.TranscationId is not { } transactionId)
            return BadRequest("Transaction ID cannot be null");

        var response = await _mediator.Send(new StopTransactionCommand(
            transactionId,
            request.TimeStop,
            request.MeterStop,
            request.StopTagId,
            request.StopReason));

        return Ok(response);
    }

    /// <summary>Returns 200 when the charger is registered, 400 otherwise; the gateway then rejects the boot.</summary>
    [HttpPut]
    public async Task<ActionResult> BootNotification(BootNotificationRequest request)
    {
        _logger.Info($"Boot notification received from: {request.OcppChargerId}, Model: {request.ChargePointModel}, Vendor: {request.ChargePointVendor}");

        var registered = await _mediator.Send(new BootNotificationCommand(request));

        return registered ? Ok(true) : BadRequest(false);
    }

    [HttpPut]
    public async Task<ActionResult> MeterValues(MeterValuesRequest request)
    {
        _logger.Info($"MeterValues received from: {request.OcppChargerId}, TransactionId: {request.TransactionId}");

        await _mediator.Send(new MeterValuesCommand(request));

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> StatusNotification(StatusNotificationRequest request)
    {
        _logger.Info($"Status notification from: {request.OcppChargerId}, Connector: {request.OcppConnectorId}, Status: {request.LastStatus}");

        await _mediator.Send(new UpdateConnectorStatusCommand
        {
            OcppChargerId = request.OcppChargerId,
            ConnectorId = request.OcppConnectorId,
            Status = request.LastStatus,
            Timestamp = request.LastStatusTime
        });

        return Ok();
    }
}
