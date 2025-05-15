using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.BusinessRules;

public class TagNotValidRule : IBusinessRule
{
    private readonly string _name;
    
    public TagNotValidRule(string name)
    {
        _name = name;
    }
   
    public bool IsBroken() => true;

    public string Message => $"Tag {_name} is not valid";
}