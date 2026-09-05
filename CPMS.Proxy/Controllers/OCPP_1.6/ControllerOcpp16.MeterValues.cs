using System.Globalization;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using Newtonsoft.Json;
using MeterValuesRequest = CPMS.Core.Models.Requests.MeterValuesRequest;

namespace CPMS.Proxy.Controllers.OCPP_1._6;

public partial class ControllerOcpp16
{
    private async Task<string?> HandleMeterValues(OCPPMessage msgIn, OCPPMessage msgOut)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<Proxy.OCPP_1._6.MeterValuesRequest>(msgIn.JsonPayload)
                          ?? throw new InvalidOperationException("Empty MeterValues payload");

            var meterValues = Summarise(request);
            meterValues.OcppChargerId = ChargePointStatus.Id;
            meterValues.Protocol = ChargePointStatus.Protocol;
            meterValues.TransactionId = request.TransactionId;

            await _cpmsClient.MeterValues(meterValues);

            msgOut.JsonPayload = JsonConvert.SerializeObject(new Proxy.OCPP_1._6.MeterValuesResponse());
            return null;
        }
        catch (Exception exp)
        {
            Logger.Error($"MeterValues => Exception: {exp.Message}", exp);
            return ErrorCodes.InternalError;
        }
    }

    /// <summary>Collapses the sampled values of one MeterValues.req into power (kW), energy (kWh) and SoC.</summary>
    private MeterValuesRequest Summarise(Proxy.OCPP_1._6.MeterValuesRequest request)
    {
        double totalPowerKw = 0;
        double totalEnergyKwh = 0;
        double stateOfCharge = 0;
        DateTimeOffset? meterTime = null;

        foreach (var meterValue in request.MeterValue)
        {
            foreach (var sample in meterValue.SampledValue)
            {
                Logger.Debug($"MeterValues => Measurand={sample.Measurand} Value={sample.Value} Unit={sample.Unit} Context={sample.Context} Location={sample.Location} Phase={sample.Phase}");

                switch (sample.Measurand)
                {
                    case SampledValueMeasurand.Power_Active_Import:
                        totalPowerKw += ToKilowatts(sample);
                        break;
                    case SampledValueMeasurand.Energy_Active_Import_Register:
                    case null: // OCPP default measurand
                        totalEnergyKwh += ToKilowattHours(sample);
                        meterTime = meterValue.Timestamp;
                        break;
                    case SampledValueMeasurand.SoC:
                        stateOfCharge = ParseOrLog(sample, "SoC");
                        break;
                }
            }
        }

        return new MeterValuesRequest
        {
            CurrentPower = totalPowerKw,
            EnergyConsumed = totalEnergyKwh,
            StateOfCharge = stateOfCharge,
            MeterTime = meterTime
        };
    }

    private double ToKilowatts(SampledValue sample)
    {
        var value = ParseOrLog(sample, "power");
        return sample.Unit switch
        {
            SampledValueUnit.W or SampledValueUnit.VA or SampledValueUnit.Var or null => value / 1000,
            SampledValueUnit.KW or SampledValueUnit.KVA or SampledValueUnit.Kvar => value,
            _ => LogUnexpectedUnit(sample)
        };
    }

    private double ToKilowattHours(SampledValue sample)
    {
        var value = ParseOrLog(sample, "energy");
        return sample.Unit switch
        {
            SampledValueUnit.Wh or SampledValueUnit.Varh or null => value / 1000,
            SampledValueUnit.KWh or SampledValueUnit.Kvarh => value,
            _ => LogUnexpectedUnit(sample)
        };
    }

    private double ParseOrLog(SampledValue sample, string what)
    {
        if (double.TryParse(sample.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return value;

        Logger.Warning($"MeterValues => invalid {what} value '{sample.Value}' (Unit={sample.Unit})");
        return 0;
    }

    private double LogUnexpectedUnit(SampledValue sample)
    {
        Logger.Warning($"MeterValues => unexpected unit '{sample.Unit}' for measurand {sample.Measurand} (Value={sample.Value})");
        return 0;
    }
}
