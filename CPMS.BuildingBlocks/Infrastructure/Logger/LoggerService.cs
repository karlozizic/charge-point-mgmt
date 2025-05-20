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
        string concisePath = GetConcisePath(filePath);
        _logger.LogInformation("{Message} at {MemberName} in {ConcisePath}:{LineNumber}", 
            message, memberName, concisePath, lineNumber);
    }

    public void Error(string message, Exception ex = null, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        string concisePath = GetConcisePath(filePath);
        _logger.LogError(ex, "{Message} at {MemberName} in {ConcisePath}:{LineNumber}", 
            message, memberName, concisePath, lineNumber);
    }

    public void Warning(string message, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        string concisePath = GetConcisePath(filePath);
        _logger.LogWarning("{Message} at {MemberName} in {ConcisePath}:{LineNumber}", 
            message, memberName, concisePath, lineNumber);
    }

    public void Debug(string message, string memberName = "", string filePath = "", int lineNumber = 0)
    {
        string concisePath = GetConcisePath(filePath);
        _logger.LogDebug("{Message} at {MemberName} in {ConcisePath}:{LineNumber}", 
            message, memberName, concisePath, lineNumber);
    }

    private static string GetConcisePath(string fullPath)
    {
        string keyFolder;
        keyFolder = fullPath.Contains("CPMS.Api") ? "CPMS.Api" : "CPMS.Proxy";
        
        int startIndex = fullPath.IndexOf(keyFolder, StringComparison.Ordinal);

        return startIndex > -1 ? fullPath.Substring(startIndex + keyFolder.Length).TrimStart('/', '\\') : fullPath;
    }
}