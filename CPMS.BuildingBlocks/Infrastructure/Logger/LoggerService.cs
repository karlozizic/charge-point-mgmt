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
        var logMessage = FormatLogMessage("INFO", message, memberName, filePath, lineNumber);
        _logger.LogInformation(logMessage);
    }

    public void Error(string message, Exception ex = null, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        var logMessage = FormatLogMessage("ERROR", message, memberName, filePath, lineNumber);
        
        if (ex != null)
        {
            var fullMessage = $"{logMessage} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}";
            _logger.LogError(fullMessage);
        }
        else
        {
            _logger.LogError(logMessage);
        }
    }

    public void Warning(string message, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        var logMessage = FormatLogMessage("WARNING", message, memberName, filePath, lineNumber);
        _logger.LogWarning(logMessage);
    }

    public void Debug(string message, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        var logMessage = FormatLogMessage("DEBUG", message, memberName, filePath, lineNumber);
        _logger.LogDebug(logMessage);
    }

    private static string FormatLogMessage(string level, string message, string memberName, string filePath, int lineNumber)
    {
        var fileName = GetFileName(filePath);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        
        return $"[{timestamp}] [{level}] {message} | Method: {memberName} | File: {fileName} | Line: {lineNumber}";
    }

    private static string GetFileName(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) 
            return "Unknown";

        try
        {
            var fileName = Path.GetFileName(fullPath);
            
            if (fullPath.Contains("CPMS.Api"))
            {
                var apiIndex = fullPath.IndexOf("CPMS.Api", StringComparison.Ordinal);
                if (apiIndex > -1)
                {
                    var relativePath = fullPath.Substring(apiIndex + "CPMS.Api".Length).TrimStart('/', '\\');
                    return $"CPMS.Api/{relativePath}";
                }
            }
            else if (fullPath.Contains("CPMS.Proxy"))
            {
                var proxyIndex = fullPath.IndexOf("CPMS.Proxy", StringComparison.Ordinal);
                if (proxyIndex > -1)
                {
                    var relativePath = fullPath.Substring(proxyIndex + "CPMS.Proxy".Length).TrimStart('/', '\\');
                    return $"CPMS.Proxy/{relativePath}";
                }
            }
            
            return fileName;
        }
        catch
        {
            return "Unknown";
        }
    }
}