using Newtonsoft.Json;
using Serilog;

namespace Logging
{
    public class Serilogger : Common.Interfaces.ILogger
    {
        private Serilog.Core.Logger _logger;
        public Serilogger(string appName)
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"logs/{appName}.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
        public void Error(string source, string message, Exception? exception = null, Dictionary<string, string>? dimensions = null)
        {
            _logger.Error($"Source: {source}, Message: {message}, Dimensions: {JsonConvert.SerializeObject(dimensions, Formatting.Indented)}, Exception type: {exception.GetType()}, Exception Message: {exception.Message}");
        }

        public void Fatal(string source, string message, Exception exception, Dictionary<string, string>? dimensions = null)
        {
            _logger.Fatal($"Source: {source}, Message: {message}, Dimensions: {JsonConvert.SerializeObject(dimensions, Formatting.Indented)}", exception);
        }

        public void Info(string source, string message)
        {
            _logger.Information($"Source: {source}, Message: {message}");
        }

        public void Warn(string source, string message, Dictionary<string, string>? dimensions = null)
        {
            _logger.Warning($"Source: {source}, Message: {message}, Dimensions: {JsonConvert.SerializeObject(dimensions, Formatting.Indented)}");
        }
    }
}
