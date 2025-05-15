using System.Globalization;
using CPMS.Core.Models.Requests;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;
using MeterValuesRequest = CPMS.Core.Models.Requests.MeterValuesRequest;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task<string?> HandleMeterValues(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        string? errorCode = null;

        try
        {
            Proxy.OCPP_1._6.MeterValuesRequest meterValueRequest = JsonConvert.DeserializeObject<Proxy.OCPP_1._6.MeterValuesRequest>(msgIn.JsonPayload) 
                                                                   ?? throw new InvalidOperationException();
            
            if (ChargePointStatus == null)
            {
                errorCode = ErrorCodes.GenericError;
                Logger.Error($"MeterValues => Unknown charge station");
                return errorCode;
            }
                
            MeterValuesRequest meterValues = ProcessMeterValues(meterValueRequest);
            meterValues.OcppChargerId = ChargePointStatus.Id;
            meterValues.Protocol = ChargePointStatus.Protocol;
            meterValues.TransactionId = meterValueRequest.TransactionId;
            
            await _cpmsClient.MeterValues(meterValues);
        }
        catch (Exception exp)
        {
            Logger.Error($"MeterValues => Exception: {exp}", exp);
            errorCode = ErrorCodes.InternalError;
        }

        return errorCode;
    }

    private MeterValuesRequest ProcessMeterValues(Proxy.OCPP_1._6.MeterValuesRequest meterValueRequest)
    {
        double totalPowerKW  = 0;
        double totalEnergyKWh  = 0;
        DateTimeOffset? meterTime = null;
        double stateOfCharge = 0;

        foreach (MeterValue meterValue in meterValueRequest.MeterValue)
        {
            foreach (SampledValue sampleValue in meterValue.SampledValue)
            {
                Logger.Info($"MeterValues => Context={sampleValue.Context} / Format={sampleValue.Format} / Value={sampleValue.Value} / Unit={sampleValue.Unit} / Location={sampleValue.Location} / Measurand={sampleValue.Measurand} / Phase={sampleValue.Phase}");
                switch (sampleValue.Measurand)
                {
                    case SampledValueMeasurand.Power_Active_Import:
                        totalPowerKW += ProcessPowerActiveImport(sampleValue);
                        break;
                    case SampledValueMeasurand.Energy_Active_Import_Register:
                        totalEnergyKWh += ProcessEnergyActiveImportRegister(sampleValue);
                        meterTime = meterValue.Timestamp;
                        break;
                    case SampledValueMeasurand.SoC:
                        stateOfCharge = ProcessStateOfCharge(sampleValue);
                        break;
                    case null:
                        totalEnergyKWh  += ProcessEnergyActiveImportRegister(sampleValue);
                        meterTime = meterValue.Timestamp;
                        break;
                }
            }
        }

        MeterValuesRequest meterValueProxy = new MeterValuesRequest
        {
            EnergyConsumed = totalEnergyKWh,
            CurrentPower = totalPowerKW,
            StateOfCharge = stateOfCharge,
            MeterTime = meterTime
        };

        return meterValueProxy;
    }

    private double ProcessPowerActiveImport(SampledValue sampleValue)
    {
        if (!double.TryParse(sampleValue.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double currentChargeKW))
        {
            return -1;
        }

        if (sampleValue.Unit == SampledValueUnit.W ||
            sampleValue.Unit == SampledValueUnit.VA ||
            sampleValue.Unit == SampledValueUnit.Var ||
            sampleValue.Unit == null)
        {
            return currentChargeKW / 1000; // convert W => kW
        }

        if (sampleValue.Unit == SampledValueUnit.KW ||
            sampleValue.Unit == SampledValueUnit.KVA ||
            sampleValue.Unit == SampledValueUnit.Kvar)
        {
            return currentChargeKW; // already kW
        }

        return -1;
    }

    private double ProcessEnergyActiveImportRegister(SampledValue sampleValue)
    {
        if (!double.TryParse(sampleValue.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double meterKWH))
        {
            Logger.Warning($"MeterValues => Value: invalid value '{sampleValue.Value}' (Unit={sampleValue.Unit})");
            return -1;
        }

        if (sampleValue.Unit == SampledValueUnit.Wh ||
            sampleValue.Unit == SampledValueUnit.Varh ||
            sampleValue.Unit == null)
        {
            return meterKWH / 1000; // convert Wh => kWh
        }

        if (sampleValue.Unit == SampledValueUnit.KWh ||
            sampleValue.Unit == SampledValueUnit.Kvarh)
        {
            return meterKWH; // already kWh
        }

        Logger.Warning($"MeterValues => Value: unexpected unit: '{sampleValue.Unit}' (Value={sampleValue.Value})");
        return -1;
    }

    private double ProcessStateOfCharge(SampledValue sampleValue)
    {
        if (!double.TryParse(sampleValue.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double stateOfCharge))
        {
            Logger.Error($"MeterValues => invalid value '{sampleValue.Value}' (SoC)");
            return -1;
        }

        return stateOfCharge;
    }
}