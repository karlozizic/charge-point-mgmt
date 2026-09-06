using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace CPMS.BuildingBlocks.Infrastructure.Logger;

/// <summary>ILogger wrapper that appends the calling member and source location to every line.</summary>
public interface ILoggerService
{
    void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
    void Error(string message, Exception? ex = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
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

    public void Info(string message, string memberName = "", string filePath = "", int lineNumber = 0) =>
        Log(LogLevel.Information, null, message, memberName, filePath, lineNumber);

    public void Error(string message, Exception? ex = null, string memberName = "", string filePath = "", int lineNumber = 0) =>
        Log(LogLevel.Error, ex, message, memberName, filePath, lineNumber);

    public void Warning(string message, string memberName = "", string filePath = "", int lineNumber = 0) =>
        Log(LogLevel.Warning, null, message, memberName, filePath, lineNumber);

    public void Debug(string message, string memberName = "", string filePath = "", int lineNumber = 0) =>
        Log(LogLevel.Debug, null, message, memberName, filePath, lineNumber);

    private void Log(LogLevel level, Exception? ex, string message, string memberName, string filePath, int lineNumber) =>
        _logger.Log(level, ex, "{Message} | {Member} | {File}:{Line}", message, memberName, Path.GetFileName(filePath), lineNumber);
}
