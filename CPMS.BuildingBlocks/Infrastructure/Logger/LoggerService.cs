using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace CPMS.BuildingBlocks.Infrastructure.Logger;

public interface ILoggerService
{
    void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
    void Error(string message, Exception ex = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
    void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
    void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
}

public class LoggerService : ILoggerService
{
    private readonly ILogger<LoggerService> _logger;

    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
    }

    public void Info(string message, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        var fileName = Path.GetFileName(filePath);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        
        _logger.LogInformation($"[{timestamp}] [INFO] {message} | {memberName} | {fileName}:{lineNumber}");
    }

    public void Error(string message, Exception ex = null, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        var fileName = Path.GetFileName(filePath);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        
        if (ex != null)
        {
            _logger.LogError(ex, $"[{timestamp}] [ERROR] {message} | {memberName} | {fileName}:{lineNumber}");
        }
        else
        {
            _logger.LogError($"[{timestamp}] [ERROR] {message} | {memberName} | {fileName}:{lineNumber}");
        }
    }

    public void Warning(string message, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        var fileName = Path.GetFileName(filePath);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        
        _logger.LogWarning($"[{timestamp}] [WARNING] {message} | {memberName} | {fileName}:{lineNumber}");
    }

    public void Debug(string message, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        var fileName = Path.GetFileName(filePath);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        
        _logger.LogDebug($"[{timestamp}] [DEBUG] {message} | {memberName} | {fileName}:{lineNumber}");
    }
}