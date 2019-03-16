using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace TempyWorker
{
    public static class TempyLogger
    {
        /// <summary>
        ///     Initialize the logger
        /// </summary>
        /// <param name="tConfiguration"></param>
        public static void Initilize(TempyConfiguration tConfiguration)
        {
            // initialize logging

            var loggingConfig = tConfiguration.GetLoggingConfig();
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                .Console(LogEventLevel.Information, theme: ConsoleTheme.None)
                .WriteTo.File(loggingConfig.LogFileName, rollingInterval: RollingInterval.Day).CreateLogger();
            Log.Debug($"Using configuration file: {tConfiguration.ConfigurationFile}");
            Log.Debug("Logging initialized");
        }
    }
}