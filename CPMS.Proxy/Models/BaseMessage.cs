namespace CPMS.Proxy.Models;

public class BaseMessage
{
    public Guid ChargerId { get; set; }
    public string Protocol { get; set; }
}