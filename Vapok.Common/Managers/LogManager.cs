using System.Collections.Generic;
using System.Diagnostics;
using BepInEx.Logging;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace Vapok.Common.Managers;

public enum LogLevels
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}
public class LogManager
{
    private static LogManager? _instance;
    private readonly Dictionary<string, Logger?> _loggers = new();


    public static void Init(string pluginName, out ILogIt? logger)
    {
        Init();
        logger = GetLogger(pluginName);
    }
    public static void Init()
    {
        if (_instance != null)
            return;
        
        _instance = new LogManager();
    }

    public static void Destroy()
    {
        if (_instance == null)
            return;
        
        foreach (var kvLogger in _instance._loggers)
            if (kvLogger.Value != null)
                kvLogger.Value.Destroy();
        
        _instance._loggers.Clear();
    }
    public static ILogIt? GetLogger(string pluginName)
    {
        if (_instance == null)
            return null;
        
        if (_instance._loggers.ContainsKey(pluginName))
            return _instance._loggers[pluginName];
        
        _instance._loggers.Add(pluginName,new Logger(pluginName));

        return _instance._loggers[pluginName];
    }
}

internal class Logger : ILogIt
{
    private readonly ManualLogSource _logger;
    public Logger(string loggerName)
    {
        _logger = BepInEx.Logging.Logger.CreateLogSource(loggerName);
    }

    public void Destroy()
    {
        BepInEx.Logging.Logger.Sources.Remove(_logger);
    }

    public void Debug(string message)
    {
        if (ConfigSyncBase.LoggingEnabled == null || ConfigSyncBase.LogLevel ==null || (ConfigSyncBase.LoggingEnabled.Value && ConfigSyncBase.LogLevel.Value <= LogLevels.Debug))
        {
            LogIt(LogLevel.Debug, message);    
        }
        
    }
    public void Info(string message)
    {
        if (ConfigSyncBase.LoggingEnabled == null || ConfigSyncBase.LogLevel ==null ||(ConfigSyncBase.LoggingEnabled.Value && ConfigSyncBase.LogLevel.Value <= LogLevels.Info))
        {
            LogIt(LogLevel.Info, message);
        }
    }
    public void Message(string message)
    {
        LogIt(LogLevel.Message, message);
    }
    public void Warning(string message)
    {
        if (ConfigSyncBase.LoggingEnabled == null || ConfigSyncBase.LogLevel ==null || (ConfigSyncBase.LoggingEnabled.Value && ConfigSyncBase.LogLevel.Value <= LogLevels.Warning))
        {
            LogIt(LogLevel.Warning, message);
        }
    }
    public void Error(string message)
    {
        if (ConfigSyncBase.LoggingEnabled == null || ConfigSyncBase.LogLevel ==null || (ConfigSyncBase.LoggingEnabled.Value && ConfigSyncBase.LogLevel.Value <= LogLevels.Error))
        {
            LogIt(LogLevel.Error, message);
        }
    }
    public void Fatal(string message)
    {
        if (ConfigSyncBase.LoggingEnabled == null || ConfigSyncBase.LogLevel ==null || (ConfigSyncBase.LoggingEnabled.Value && ConfigSyncBase.LogLevel.Value <= LogLevels.Fatal))
        {
            LogIt(LogLevel.Fatal, message);    
        }
    }

    private void LogIt(LogLevel level, string message)
    {
        var callingType = new StackFrame(2).GetMethod().DeclaringType;
        var callingMethod = new StackFrame(2).GetMethod().Name;
        
        _logger.Log(level, $"[{callingType}.{callingMethod}] {message}");
    }
}

