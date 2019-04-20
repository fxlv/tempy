using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TempyConfiguration;

namespace TempyLogger
{
    public static class Logger
    {
        /// <summary>
        ///     Initialize the logger
        /// </summary>
        /// <param name="tConfiguration"></param>
        public static void Initilize(Configuration tConfiguration)
        {
            // initialize logging

            var loggingConfig = tConfiguration.GetLoggingConfig();
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                .Console(LogEventLevel.Information, theme: ConsoleTheme.None)
                .WriteTo.File(loggingConfig.LogFilePath, rollingInterval: RollingInterval.Day).CreateLogger();
            Log.Debug($"Using configuration file: {tConfiguration.ConfigurationFile}");
            Log.Information($"Logging initialized, writing DEBUG logs to {loggingConfig.LogFilePath}");
        }

       
    }
}