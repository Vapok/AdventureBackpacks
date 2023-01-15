using System.Collections.Generic;
using System.Diagnostics;
using BepInEx.Logging;
using Vapok.Common.Abstractions;

namespace Vapok.Common.Managers;

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
        LogIt(LogLevel.Debug, message);
    }
    public void Info(string message)
    {
        LogIt(LogLevel.Info, message);
    }
    public void Message(string message)
    {
        LogIt(LogLevel.Message, message);
    }
    public void Warning(string message)
    {
        LogIt(LogLevel.Warning, message);
    }
    public void Error(string message)
    {
        LogIt(LogLevel.Error, message);
    }
    public void Fatal(string message)
    {
        LogIt(LogLevel.Fatal, message);
    }

    private void LogIt(LogLevel level, string message)
    {
        var callingType = new StackFrame(2).GetMethod().DeclaringType;
        var callingMethod = new StackFrame(2).GetMethod().Name;
        
        _logger.Log(level, $"[{callingType}.{callingMethod}] {message}");
    }
}

