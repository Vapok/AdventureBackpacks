namespace Vapok.Common.Abstractions;

public interface ILogIt
{
    void Debug(string message);
    void Info(string message);
    void Message(string message);
    void Warning(string message);
    void Error(string message);
    void Fatal(string message);
}