using Microsoft.Extensions.Logging;

namespace AhAnalyticsPriceUpdater;

public static class LogProvider
{
    private static ILogger? logger;

    private static void InitializeLogging()
    {
        var logFilePath = Directory.GetCurrentDirectory();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            var logFileWriter = new StreamWriter(logFilePath, true);

            builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
        });

        logger ??= loggerFactory.CreateLogger(nameof(ScanDataDecrypter));
    }

    public static ILogger? GetLogger()
    {
        InitializeLogging();

        return logger;
    }
}

internal class CustomFileLoggerProvider(StreamWriter logFileWriter) : ILoggerProvider
{
    private readonly StreamWriter logFileWriter = logFileWriter ?? throw new ArgumentNullException(nameof(logFileWriter));

    public ILogger CreateLogger(string categoryName) => new CustomFileLogger(categoryName, logFileWriter);

    public void Dispose() => logFileWriter.Dispose();
}

internal class CustomFileLogger(string categoryName, StreamWriter logFileWriter) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel                        logLevel,
                            EventId                         eventId,
                            TState                          state,
                            Exception                       exception,
                            Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);

        logFileWriter.WriteLine($"[{DateTime.Now:dd.MM.yyyy hh:mm:ss.fff}] [{logLevel}] [{categoryName}] {message}");
        logFileWriter.Flush();
    }
}