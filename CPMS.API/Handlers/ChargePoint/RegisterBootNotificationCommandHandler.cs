using CPMS.API.Repositories;
using CPMS.Core.Models.Requests;
using MediatR;

namespace CPMS.API.Handlers.ChargePoint;

public class BootNotificationCommand : IRequest<bool>
{
    public string OcppChargerId { get; set; }
    public string Protocol { get; set; }
    public string ChargePointVendor { get; set; }
    public string ChargePointModel { get; set; }
    public string ChargePointSerialNumber { get; set; }
    public string ChargeBoxSerialNumber { get; set; }
    public string FirmwareVersion { get; set; }
    public string Iccid { get; set; }
    public string Imsi { get; set; }
    public string MeterType { get; set; }
    public string MeterSerialNumber { get; set; }

    public BootNotificationCommand(BootNotificationRequest bootNotificationRequest)
    {
        OcppChargerId = bootNotificationRequest.OcppChargerId;
        Protocol = bootNotificationRequest.Protocol;
        ChargePointVendor = bootNotificationRequest.ChargePointVendor;
        ChargePointModel = bootNotificationRequest.ChargePointModel;
        ChargePointSerialNumber = bootNotificationRequest.ChargePointSerialNumber;
        ChargeBoxSerialNumber = bootNotificationRequest.ChargeBoxSerialNumber;
        FirmwareVersion = bootNotificationRequest.FirmwareVersion;
        Iccid = bootNotificationRequest.Iccid;
        Imsi = bootNotificationRequest.Imsi;
        MeterType = bootNotificationRequest.MeterType;
        MeterSerialNumber = bootNotificationRequest.MeterSerialNumber;
    }
}

public class RegisterBootNotificationCommandHandler : IRequestHandler<BootNotificationCommand, bool>
{
    private readonly IChargePointRepository _chargePointRepository;
        
    public RegisterBootNotificationCommandHandler(IChargePointRepository chargePointRepository)
    {
        _chargePointRepository = chargePointRepository;
    }
    
    public async Task<bool> Handle(BootNotificationCommand request, CancellationToken cancellationToken)
    {
        var chargePoint = await _chargePointRepository.GetByOcppChargerIdAsync(request.OcppChargerId);
            
        if (chargePoint == null)
            return false;
            
        chargePoint.RegisterBoot(
            request.ChargePointSerialNumber,
            request.ChargePointModel,
            request.ChargePointVendor,
            request.FirmwareVersion);
                
        await _chargePointRepository.UpdateAsync(chargePoint);
            
        return true;
    }
}