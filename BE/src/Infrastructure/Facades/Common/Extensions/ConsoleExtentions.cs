using System.Diagnostics;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions
{
    public static class ConsoleExtentions
    {
        public static char GetChar(int countdownTime, char defaultChar)
        {
            ConsoleKeyInfo info;

            Stopwatch stopwatch = new();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < countdownTime)
            {
                if (Console.KeyAvailable)
                {
                    info = Console.ReadKey();
                    return info.KeyChar;
                }

                // Sleep for a short interval to avoid busy-waiting
                Thread.Sleep(100);
            }

            return defaultChar;
        }
    }
}