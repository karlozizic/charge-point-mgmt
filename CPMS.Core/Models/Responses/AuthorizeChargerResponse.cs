
namespace CPMS.Core.Models.Responses;

public class AuthorizeChargerResponse
{ 
    public AuthorizationStatus AuthorizationStatus { get; set; }
}

public enum AuthorizationStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Blocked")]
    Blocked = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"Expired")]
    Expired = 2,

    [System.Runtime.Serialization.EnumMember(Value = @"Invalid")]
    Invalid = 3,

    [System.Runtime.Serialization.EnumMember(Value = @"ConcurrentTx")]
    ConcurrentTx = 4,
}