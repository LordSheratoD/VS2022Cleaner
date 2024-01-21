using System;

namespace VS2022Cleaner
{
    public static class Logger
    {
        public enum LogLevel
        {
            TRACE,
            DEBUG,
            INFO,
            WARN,
            ERROR,
            FATAL
        }

        public static void Log(LogLevel level, string message)
        {
            SetConsoleColor(level);
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
            Console.ResetColor(); // Restablece al color predeterminado
        }

        private static void SetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.TRACE:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.DEBUG:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogLevel.WARN:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.FATAL:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }
    }
}