using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.BusinessRules;

public class CannotStopAlreadyStoppedSessionRule : IBusinessRule
{
    public bool IsBroken() => true;

    public string Message => "Cannot stop an already stopped session.";
}