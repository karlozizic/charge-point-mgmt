using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.BusinessRules;

public class CannotAddMeterValuesAfterStopRule : IBusinessRule
{
    public bool IsBroken() => true;

    public string Message => "Cannot add meter values after stop";
}