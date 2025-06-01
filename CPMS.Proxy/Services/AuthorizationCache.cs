using CPMS.BuildingBlocks.Infrastructure.Logger;
using Microsoft.Extensions.Caching.Memory;

namespace CPMS.Proxy.Services;

public class AuthorizationCache : IAuthorizationCache
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _authorizationTimeout;
    private readonly ILoggerService _logger;
    
    public AuthorizationCache(IMemoryCache cache, 
        ILoggerService logger,
        IConfiguration configuration)
    {
        _cache = cache;
        _logger = logger;
        
        var timeoutMinutes = configuration.GetValue("Authorization:TimeoutMinutes", 5);
        _authorizationTimeout = TimeSpan.FromMinutes(timeoutMinutes);
    }
    
    public void AddAuthorization(string chargePointId, string tagId)
    {
        var key = GenerateKey(chargePointId, tagId);
        
        _cache.Set(key, true, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _authorizationTimeout,
            Priority = CacheItemPriority.Normal
        });
        
        _logger.Info($"Authorization cached for ChargePoint: {chargePointId}, Tag: {tagId}, Expires in: {_authorizationTimeout.TotalMinutes} minutes");
    }
    
    public bool IsAuthorized(string chargePointId, string tagId)
    {
        var key = GenerateKey(chargePointId, tagId);
        var isAuthorized = _cache.TryGetValue(key, out _);

        _logger.Debug(isAuthorized
            ? $"Authorization found for ChargePoint: {chargePointId}, Tag: {tagId}"
            : $"No authorization found for ChargePoint: {chargePointId}, Tag: {tagId}");

        return isAuthorized;
    }
    
    private string GenerateKey(string chargePointId, string tagId)
    {
        return $"auth:{chargePointId}:{tagId}";
    }
}

public interface IAuthorizationCache
{
    void AddAuthorization(string chargePointId, string tagId);
    bool IsAuthorized(string chargePointId, string tagId);
}