namespace CPMS.Proxy.Models.Cpms.Requests;

public class AuthorizeChargerCpmsRequest : BaseMessage
{
    public string IdTag { get; set; }
}