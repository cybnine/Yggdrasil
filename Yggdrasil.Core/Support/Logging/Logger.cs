using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Yggdrasil.Core.Support.Logging
{
    public enum LogLevel
    {
        Verbose,
        Debug,
        Info,
        Warning,
        Error
    }

    public class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        public static Logger Instance => _instance.Value;

        private readonly ConcurrentQueue<(LogLevel Level, string Message)> _logQueue = new ConcurrentQueue<(LogLevel, string)>();
        private readonly ManualResetEventSlim _logEvent = new ManualResetEventSlim(false);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public bool VerboseLoggingEnabled { get; set; } = false;

        private Logger()
        {
            Task.Run(ProcessLogQueue);
        }

        public void Log(LogLevel level, string message)
        {
            if (level == LogLevel.Verbose && !VerboseLoggingEnabled)
                return;

            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
            _logQueue.Enqueue((level, logMessage));
            _logEvent.Set();
        }

        public void LogVerbose(string message)
        {
            Log(LogLevel.Verbose, message);
        }

        private async Task ProcessLogQueue()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Run(() => _logEvent.Wait(_cts.Token));
                
                while (_logQueue.TryDequeue(out var logEntry))
                {
                    WriteColoredLog(logEntry.Level, logEntry.Message);
                }

                _logEvent.Reset();
            }
        }

        private void WriteColoredLog(LogLevel level, string message)
        {
            Console.ResetColor();
            Console.Write($"{message.Substring(0, message.IndexOf(']') + 1)} ");

            Console.ForegroundColor = level switch
            {
                LogLevel.Verbose => ConsoleColor.Magenta,
                LogLevel.Debug => ConsoleColor.Cyan,
                LogLevel.Info => ConsoleColor.Green,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            Console.WriteLine(message.Substring(message.IndexOf(']') + 1));
            Console.ResetColor();
        }

        public void Shutdown()
        {
            _cts.Cancel();
        }
    }
}