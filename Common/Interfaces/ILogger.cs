namespace Common.Interfaces
{
    public interface ILogger
    {
        void Info(string source, string message);
        void Warn(string source, string message, Dictionary<string, string>? dimensions = null);
        void Error(string source, string message, Exception? exception = null, Dictionary<string, string>? dimensions = null);
        void Fatal(string source, string message, Exception exception, Dictionary<string, string>? dimensions = null);
    }
}
