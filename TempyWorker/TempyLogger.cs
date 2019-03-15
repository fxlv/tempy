using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace TempyWorker
{
    public static class TempyLogger
    {
        
      
        public static void Initilize(TempyConfiguration tConfiguration)
        {
            // initialize logging

            var loggingConfig = GetLoggingConfig(tConfiguration.ConfigurationRoot);
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                .Console(LogEventLevel.Information, theme: ConsoleTheme.None)
                .WriteTo.File(loggingConfig.LogFileName, rollingInterval: RollingInterval.Day).CreateLogger();
            Log.Debug("Logging initialized");
        }

        public static LoggingConfig GetLoggingConfig(IConfigurationRoot configuration)
        {
            var loggingConfig = new LoggingConfig();
            configuration.GetSection("logging").Bind(loggingConfig);
            return loggingConfig;
        }
    }


    public class LoggingConfig
    {
        public string LogDirectory { get; set; }
        public string LogFileName { get; set; }

    }
}