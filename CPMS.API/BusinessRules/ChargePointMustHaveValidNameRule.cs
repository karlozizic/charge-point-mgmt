using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.DomainRules;

public class ChargePointMustHaveValidNameRule : IBusinessRule
{
    private readonly string _name;
    
    public ChargePointMustHaveValidNameRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    public string Message => "Charge point must have valid name!";
}