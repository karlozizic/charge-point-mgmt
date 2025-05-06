namespace CPMS.Core.Models.Requests;

public class AuthorizeChargerRequest : BaseMessage
{
    public string IdTag { get; set; }
}